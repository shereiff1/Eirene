using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class DoctorRatingService : IDoctorRatingService
    {
        private readonly ILogger<DoctorRatingService> _logger;
        private readonly IDoctorRatingRepository _ratingRepository;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IPatientProfileRepository _patientProfileRepository;

        public DoctorRatingService(
            ILogger<DoctorRatingService> logger,
            IDoctorRatingRepository ratingRepository,
            IDoctorProfileRepository doctorProfileRepository,
            IPatientProfileRepository patientProfileRepository)
        {
            _logger = logger;
            _ratingRepository = ratingRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _patientProfileRepository = patientProfileRepository;
        }

        public async Task<Result<List<DoctorRatingDTO>>> GetDoctorRatingsAsync(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return Result.Failure<List<DoctorRatingDTO>>("Doctor not found.");
                }

                var ratings = await _ratingRepository.FindAsync(r => r.DoctorProfileId == doctorId);

                var ratingDtos = new List<DoctorRatingDTO>();
                foreach (var r in ratings)
                {
                    var patient = await _patientProfileRepository.GetByIdAsync(r.PatientProfileId);
                    ratingDtos.Add(new DoctorRatingDTO
                    {
                        Id = r.Id,
                        PatientProfileId = r.PatientProfileId,
                        PatientName = patient?.User?.FullName ?? "Unknown",
                        Rating = r.Rating,
                        Review = r.Review,
                        CreatedAt = r.CreatedAt
                    });
                }

                return Result.Success(ratingDtos.OrderByDescending(r => r.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings for doctor {DoctorId}", doctorId);
                return Result.Failure<List<DoctorRatingDTO>>("An error occurred while fetching ratings.");
            }
        }
    }
}
