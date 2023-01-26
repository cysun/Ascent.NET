using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class DistributionController : Controller
    {
        private readonly MftService _mftService;

        private readonly IMapper _mapper;
        private readonly ILogger<DistributionController> _logger;

        public DistributionController(MftService mftService, IMapper mapper, ILogger<DistributionController> logger)
        {
            _mftService = mftService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(int? year)
        {
            var years = _mftService.GetDistributionYears();
            if (years.Count == 0) return View(new List<MftDistribution>());

            ViewBag.SelectedYear = year ?? years[0];
            ViewBag.Years = years.Select(y => new SelectListItem()
            {
                Text = y.ToString(),
                Value = y.ToString(),
                Selected = y == ViewBag.SelectedYear
            });

            return View(_mftService.GetDistributions(ViewBag.SelectedYear));
        }

        public IActionResult View(int id)
        {
            var distribution = _mftService.GetDistribution(id);
            if (distribution == null) return NotFound();

            return View(distribution);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            var distributionTypes = _mftService.GetDistributionTypes();

            ViewBag.DistributionTypes = distributionTypes.Select(t => new SelectListItem()
            {
                Text = t.Name,
                Value = t.Alias
            });

            return View(new MftDistributionInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(MftDistributionInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            if (_mftService.GetDistribution(input.Year, input.TypeAlias) != null)
                return BadRequest();

            var distribution = _mapper.Map<MftDistribution>(input, opts => opts.Items["type"] = input.TypeAlias);
            _mftService.AddDistribution(distribution);
            _logger.LogInformation("{user} added mft distribution {distribution}", User.Identity.Name, distribution.Id);

            return RedirectToAction("View", new { id = distribution.Id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            _mftService.DeleteDistribution(id);
            _logger.LogInformation("{user} added mft distribution {distribution}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class MftDistributionInputModel
    {
        public int Year { get; set; } = DateTime.Now.Year;

        [Display(Name = "Type")]
        public string TypeAlias { get; set; }

        [Display(Name = "From Date")]
        public DateOnly FromDate { get; set; } = new DateOnly(2015, 9, 1);
        [Display(Name = "To Date")]
        public DateOnly ToDate { get; set; } = new DateOnly(DateTime.Now.Year, 6, 30);

        [Display(Name = "Name of Samples")]
        public int NumOfSamples { get; set; }

        public double Mean { get; set; }
        public double Median { get; set; }

        [Display(Name = "Standard Deviation")]
        public double StdDev { get; set; }

        public string Ranks { get; set; }
    }
}
