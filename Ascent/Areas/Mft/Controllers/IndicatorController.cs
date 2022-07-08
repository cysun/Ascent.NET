using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class IndicatorController : Controller
    {
        private readonly MftService _mftService;

        private readonly IMapper _mapper;
        private readonly ILogger<IndicatorController> _logger;

        public IndicatorController(MftService mftService, IMapper mapper, ILogger<IndicatorController> logger)
        {
            _mftService = mftService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_mftService.GetIndicators());
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new MftIndicatorInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(MftIndicatorInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var indicator = _mftService.GetIndicator(input.Year);
            if (indicator == null)
            {
                indicator = _mapper.Map<MftIndicator>(input);
                _mftService.AddIndicator(indicator);
                _logger.LogInformation("{user} added indicators for year {year}", User.Identity.Name, input.Year);
            }
            else
            {
                _mapper.Map(input, indicator);
                indicator.Percentiles = new int?[] { null, null, null };
                _mftService.SaveChanges();
                _logger.LogInformation("{user} updated indicators for year {year}", User.Identity.Name, input.Year);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult UpdatePercentiles()
        {
            var distributionYears = _mftService.GetDistributionYears();
            if (distributionYears.Count == 0)
                return RedirectToAction("Index");

            var indicators = _mftService.GetIndicators();
            foreach (var indicator in indicators)
                UpdatePercentiles(indicator, distributionYears);

            _mftService.SaveChanges();
            _logger.LogInformation("{user} updated percentiles for indicators", User.Identity.Name);

            return RedirectToAction("Index");
        }

        private void UpdatePercentiles(MftIndicator indicator, List<int> distributionYears)
        {
            var distributionYear = distributionYears[0];
            foreach (var y in distributionYears)
                if (Math.Abs(y - indicator.Year) < Math.Abs(distributionYear - indicator.Year))
                    distributionYear = y;
            _logger.LogDebug("Selected distribution {distributionYear} for {year}", distributionYear, indicator.Year);

            var distribution = _mftService.GetDistribution(distributionYear, "AI1");
            if (distribution != null)
                indicator.Percentiles[0] = distribution.GetPercentile(indicator.Scores[0]);

            distribution = _mftService.GetDistribution(distributionYear, "AI2");
            if (distribution != null)
                indicator.Percentiles[1] = distribution.GetPercentile(indicator.Scores[1]);

            distribution = _mftService.GetDistribution(distributionYear, "AI3");
            if (distribution != null)
                indicator.Percentiles[2] = distribution.GetPercentile(indicator.Scores[2]);
        }
    }
}

namespace Ascent.Models
{
    public class MftIndicatorInputModel
    {
        public int Year { get; set; }

        [Display(Name = "Students")]
        public int NumOfStudents { get; set; }

        public int[] Scores { get; set; } = new int[3];
    }
}
