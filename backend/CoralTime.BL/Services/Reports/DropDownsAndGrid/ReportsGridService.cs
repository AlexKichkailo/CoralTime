﻿using CoralTime.Common.Exceptions;
using CoralTime.Common.Helpers;
using CoralTime.DAL.ConvertersViews.ExstensionsMethods;
using CoralTime.DAL.Models;
using CoralTime.ViewModels.Reports;
using CoralTime.ViewModels.Reports.Request.ReportsGrid;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static CoralTime.Common.Constants.Constants;

namespace CoralTime.BL.Services.Reports.DDAndGrid
{
    public partial class ReportService
    {
        #region Get DropDowns and Grid. Filtration By / Grouping By: None, Projects, Users, Dates, Clients.

        public ReportsGrandGridTimeEntryView GroupByNone(string userName, RequestReportsGrid reportsGridData)
        {
            var reportsGridTimeEntry = new ReportsGrandGridTimeEntryView
            {
                ReportsGridView = new List<ReportGridTimeEntryView>
                {
                    new ReportGridTimeEntryView
                    {
                        Items = new List<ReportsGridItemsView>()
                    }
                }
            };

            var timeEntriesForGrouping = GetTimeEntriesForGrouping(userName, reportsGridData);
            if (!timeEntriesForGrouping.Any())
            {
                return reportsGridTimeEntry;
            }

            var timeEntriesGroupByNone = timeEntriesForGrouping
                .GroupBy(x => x.Id)
                .ToDictionary(key => key.Key, key => key.Select(value => value));

            var result = reportsGridTimeEntry.GetViewReportsGrandGridTimeEntries(timeEntriesGroupByNone, Mapper);

            return result;
        }

        public ReportsGrandGridProjectsView GroupByProjects(string userName, RequestReportsGrid reportsGridData)
        {
            var reportsGridProjects = new ReportsGrandGridProjectsView
            {
                ReportsGridView = new List<ReportGridProjectView>
                {
                    new ReportGridProjectView
                    {
                        Items = new List<ReportsGridItemsView>()
                    }
                }
            };

            var timeEntriesForGrouping = GetTimeEntriesForGrouping(userName, reportsGridData);
            if (!timeEntriesForGrouping.Any())
            {
                return reportsGridProjects;
            }

            var timeEntriesGroupByProjects = timeEntriesForGrouping
                .GroupBy(i => i.Project)
                .OrderBy(x => x.Key.Name)
                .ToDictionary(key => key.Key, key => key.Select(value => value));

            var result = reportsGridProjects.GetViewReportsGrandGridClients(timeEntriesGroupByProjects, Mapper);

            return result;
        }

        public ReportsGrandGridMembersView GroupByUsers(string userName, RequestReportsGrid reportsGridData)
        {
            var reportsGridUsers = new ReportsGrandGridMembersView
            {
                ReportsGridView = new List<ReportGridMemberView>
                {
                    new ReportGridMemberView
                    {
                        Items = new List<ReportsGridItemsView>()
                    }
                }
            };

            var timeEntriesForGrouping = GetTimeEntriesForGrouping(userName, reportsGridData);
            if (!timeEntriesForGrouping.Any())
            {
                return reportsGridUsers;
            }

            var timeEntriesGroupByUsers = timeEntriesForGrouping
                .GroupBy(i => i.Member)
                .OrderBy(x => x.Key.FullName)
                .ToDictionary(key => key.Key, key => key.Select(value => value));

            var result = reportsGridUsers.GetViewReportsGrandGridClients(timeEntriesGroupByUsers, Mapper);

            return result;
        }

        public ReportsGrandGridDatesView GroupByDates(string userName, RequestReportsGrid reportsGridData)
        {
            var reportsGridDates = new ReportsGrandGridDatesView
            {
                ReportsGridView = new List<ReportGridDateView>
                {
                    new ReportGridDateView
                    {
                        Items = new List<ReportsGridItemsView>()
                    }
                }
            };

            var timeEntriesForGrouping = GetTimeEntriesForGrouping(userName, reportsGridData);
            if (!timeEntriesForGrouping.Any())
            {
                return reportsGridDates;
            }

            var timeEntriesGroupByDate = timeEntriesForGrouping
                .GroupBy(i => i.Date)
                .ToDictionary(key => key.Key, key => key.Select(value => value));

            var result = reportsGridDates.GetViewReportsGrandGridClients(timeEntriesGroupByDate, Mapper);

            return result;
        }

        public ReportsGrandGridClients GroupByClients(string userName, RequestReportsGrid reportsGridData)
        {
            var reportsGridClients = new ReportsGrandGridClients
            {
                ReportsGridView = new List<ReportGridClientView>
                {
                    new ReportGridClientView
                    {
                        Items = new List<ReportsGridItemsView>()
                    }
                }
            };

            var timeEntriesForGrouping = GetTimeEntriesForGrouping(userName, reportsGridData);
            if (!timeEntriesForGrouping.Any())
            {
                return reportsGridClients;
            }

            var timeEntriesGroupByClients = timeEntriesForGrouping
                .GroupBy(i => i.Project.Client)
                .OrderBy(x => x.Key.Name)
                .ToDictionary(key => key.Key ?? CreateWithOutClientInstance(), key => key.Select(value => value));

            var result = reportsGridClients.GetViewReportsGrandGridClients(timeEntriesGroupByClients, Mapper);

            return result;
        }

        #endregion

        #region Get DropDowns and Grid. Filtration By / Grouping By: None, Projects, Users, Dates, Clients. (Common methods)

        private IQueryable<TimeEntry> GetTimeEntriesForGrouping(string userName, RequestReportsGrid reportsGridData)
        {
            var currentMember = Uow.MemberRepository.LinkedCacheGetByName(userName);

            if (currentMember == null)
            {
                throw new CoralTimeEntityNotFoundException($"Member with userName = {userName} not found.");
            }

            if (!currentMember.User.IsActive)
            {
                throw new CoralTimeEntityNotFoundException($"Member with userName = {userName} is not active.");
            }

            CommonHelpers.SetRangeOfWeekByDate(out var weekStart, out var weekEnd, DateTime.Now);

            DateFrom = reportsGridData?.DateFrom ?? weekStart;
            DateTo = reportsGridData?.DateTo ?? weekEnd;

            // By Dates (default grouping, i.e. "Group by None"; direct order).
            var timeEntriesByDateOfUser = GetTimeEntryByDate(currentMember, DateFrom, DateTo);

            // By Projects.
            if (reportsGridData?.ProjectIds != null && reportsGridData.ProjectIds.Length > 0)
            {
                timeEntriesByDateOfUser = timeEntriesByDateOfUser.Where(x => reportsGridData.ProjectIds.Contains(x.ProjectId));
            }

            // By Members.
            if (reportsGridData?.MemberIds != null && reportsGridData.MemberIds.Length > 0)
            {
                timeEntriesByDateOfUser = timeEntriesByDateOfUser.Where(x => reportsGridData.MemberIds.Contains(x.MemberId));
            }

            // By Clients that has Projects.
            if (reportsGridData?.ClientIds != null && reportsGridData.ClientIds.Length > 0)
            {
                timeEntriesByDateOfUser = timeEntriesByDateOfUser.Where(x => reportsGridData.ClientIds.Contains(x.Project.ClientId) || x.Project.ClientId == null && reportsGridData.ClientIds.Contains(WithoutClient.Id));
            }

            return timeEntriesByDateOfUser;
        }

        private IQueryable<TimeEntry> GetTimeEntryByDate(Member currentMember, DateTime dateFrom, DateTime dateTo)
        {
            // #0 Get timeEntriesByDate.s
            var timeEntriesByDate = Uow.TimeEntryRepository.GetQueryWithIncludes()
                .Include(x => x.Project).ThenInclude(x => x.Client)
                .Include(x => x.Member.User)
                .Include(x => x.TaskType)
                .Where(t => t.Date.Date >= dateFrom.Date && t.Date.Date <= dateTo.Date);

            #region Constrain for Admin: return all TimeEntries.

            if (currentMember.User.IsAdmin)
            {
                return timeEntriesByDate;
            }

            #endregion

            #region Constrain for Member. return only TimeEntries that manager is assign.

            if (!currentMember.User.IsAdmin && !currentMember.User.IsManager)
            {
                // #1. TimeEntries. Get tEntries for this member.
                timeEntriesByDate = timeEntriesByDate.Where(t => t.MemberId == currentMember.Id);
            }

            #endregion

            #region Constrain for Manager : return #1 TimeEntries that currentMember is assign, #2 TimeEntries for not assign users at Projects (but TEntries was saved), #4 TimeEntries with global projects that not contains in result.

            if (!currentMember.User.IsAdmin && currentMember.User.IsManager)
            {
                var managerRoleId = Uow.ProjectRoleRepository.LinkedCacheGetList().FirstOrDefault(r => r.Name == ManagerRole).Id;

                var managerProjectIds = Uow.MemberProjectRoleRepository.LinkedCacheGetList()
                    .Where(r => r.MemberId == currentMember.Id && r.RoleId == managerRoleId)
                    .Select(x => x.ProjectId)
                    .ToArray();

                // #1. TimeEntries. Get tEntries for this member and tEntries that is current member is Manager!.
                timeEntriesByDate = timeEntriesByDate.Where(t => t.MemberId == currentMember.Id || managerProjectIds.Contains(t.ProjectId));
            }

            return timeEntriesByDate;

            #endregion
        }

        private Client CreateWithOutClientInstance()
        {
            var getAdminUserById = Uow.UserRepository.LinkedCacheGetList().FirstOrDefault(x => x.Id == "038d14e5-27ef-4b07-89b5-39ea8ed0cbf7");

            var withoutClient = new Client
            {
                Id = WithoutClient.Id,
                Creator = getAdminUserById,
                LastEditor = getAdminUserById
            };

            return withoutClient;
        }

        #endregion
    }
}