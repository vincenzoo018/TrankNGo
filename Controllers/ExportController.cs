using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    public class ExportController : BaseController
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost]
        public async Task<IActionResult> ExportData(ExportRequestViewModel model)
        {
            var user = GetCurrentUser();
            // RBAC: Only Oversight Officer can export data
            if (user == null || user.Role != UserRole.OversightOfficer)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid export request.";
                return RedirectToAction("Export", "Oversight");
            }

            // 1. Validate secondary export password
            var isValid = await _exportService.ValidateExportPasswordAsync(user.Id, model.ExportPassword);
            if (!isValid)
            {
                TempData["ErrorMessage"] = "Invalid export password. Access denied.";
                await _exportService.LogExportAsync(user.Id, model.ExportType, "Failed Attempt", 0, "Failed");
                return RedirectToAction("Export", "Oversight");
            }

            // 2. Parse dates
            DateTime? from = null;
            DateTime? to = null;
            if (DateTime.TryParse(model.FilterDateFrom, out var df)) from = df;
            if (DateTime.TryParse(model.FilterDateTo, out var dt)) to = dt;

            // 3. Generate CSV
            var csvData = await _exportService.ExportDocumentsCSVAsync(
                model.FilterDocType, model.FilterDepartment, model.FilterStatus, from, to);

            // 4. Log successful export
            var scope = $"Type: {model.FilterDocType ?? "All"}, Dept: {model.FilterDepartment ?? "All"}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            await _exportService.LogExportAsync(user.Id, "CSV", scope, bytes.Length, "Success");

            return File(bytes, "text/csv", $"TrackNGo_Export_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
