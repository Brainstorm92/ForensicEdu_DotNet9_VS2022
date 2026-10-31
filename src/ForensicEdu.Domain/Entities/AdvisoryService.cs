using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class AdvisoryService
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "Asesoría Personalizada Pericial y Consultoría en Juicio Oral";
    public string Specialist { get; set; } = "Cmdte. (R) Manuel Sotomayor Vega";
    public string Specialty { get; set; } = "Perito Oficial Balístico & Criminalista Forense";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 250.00m;
    public string WhatsappContact { get; set; } = "+51 944 678 901";
}
