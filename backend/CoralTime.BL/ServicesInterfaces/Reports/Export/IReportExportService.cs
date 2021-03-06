﻿using System;
using CoralTime.ViewModels.Reports.Request.ReportsEmails;
using CoralTime.ViewModels.Reports.Request.ReportsGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoralTime.BL.ServicesInterfaces.Reports.Export
{
    public interface IReportExportService
    {
        // Export Excel, CSV, PDF. Filtration By / Grouping By: None, Projects, Users, Dates, Clients.
        FileResult ExportGroupByNone(string userName, RequestReportsGrid reportsGridData, HttpContext httpContext);

        FileResult ExportGroupByProjects(string userName, RequestReportsGrid reportsGridData, HttpContext httpContext);

        FileResult ExportGroupByUsers(string userName, RequestReportsGrid reportsGridData, HttpContext httpContext);

        FileResult ExportGroupByDates(string userName, RequestReportsGrid reportsGridData, HttpContext httpContext);

        FileResult ExportGroupByClients(string userName, RequestReportsGrid reportsGridData, HttpContext httpContext);

        // Sent reports as email.
        Task SentGroupByNone(string userName, ReportsSendAsEmailView emailData);

        Task SentGroupByProjects(string userName, ReportsSendAsEmailView emailData);

        Task SentGroupByUsers(string userName, ReportsSendAsEmailView emailData);

        Task SentGroupByDates(string userName, ReportsSendAsEmailView emailData);

        Task SentGroupByClients(string userName, ReportsSendAsEmailView emailData);
    }
}