using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Policies;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Services;

public static class CourseEnrollmentRules
{
    public static async Task<(bool Allowed,List<Enrollment> Previous)> CheckAsync(IApplicationDbContext db,string userId,string courseId,CancellationToken ct)
    {
        var previous=await db.Enrollments.Where(e=>e.UserId==userId && e.CourseId==courseId).ToListAsync(ct);
        if(previous.Count==0)return (true,previous);
        var exhausted=await db.Enrollments.Where(e=>e.UserId==userId && e.CourseId==courseId)
            .Where(EnrollmentPolicy.Exhausted).Select(e=>e.Id).ToListAsync(ct);
        return (previous.All(e=>exhausted.Contains(e.Id)),previous);
    }
    public const string UnavailableMessage="Ya tienes una matrícula con intentos disponibles, aprobada o bloqueada. Solo puedes solicitar una nueva matrícula después de agotar los dos intentos sin aprobar.";
}
