using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// Fixed, fictitious content for "Cedarline Health", a made-up network of three family clinics: clinics, doctors and their
/// weekly hours, patients, appointments, lab orders, prescriptions, and bills. Every fresh tab starts the same way. Nothing
/// here is medical advice.
/// </summary>
public static class HcSeedData
{
    #region Fields and Constants

    /// <summary>The identity role of doctors.</summary>
    public const string DoctorRole = "doctor";

    /// <summary>The identity role of the front desk.</summary>
    public const string FrontDeskRole = "frontdesk";

    /// <summary>The visit fee for a lab test, in cents.</summary>
    public const int LabFeeCents = 2_500;

    /// <summary>The identity role of patients (and of new accounts).</summary>
    public const string PatientRole = "patient";

    /// <summary>The fictitious clinic network.</summary>
    public const string ProductName = "Cedarline Health";

    /// <summary>The demo doctor's ID.</summary>
    public const string SeededDoctorId = "okoye";

    /// <summary>The demo front desk account's ID.</summary>
    public const string SeededFrontDeskId = "desk";

    /// <summary>The demo patient's account ID.</summary>
    public const string SeededPatientAccountId = "maya";

    /// <summary>The demo patient's record ID.</summary>
    public const int SeededPatientId = 1;

    /// <summary>The staff roles (doctors and the front desk).</summary>
    public const string StaffRoles = DoctorRole + "," + FrontDeskRole;

    /// <summary>The visit fee for family medicine and pediatrics, in cents.</summary>
    public const int PrimaryCareFeeCents = 4_000;

    /// <summary>The visit fee for specialists, in cents.</summary>
    public const int SpecialistFeeCents = 6_000;

    /// <summary>When a fresh tab's clock starts (a Tuesday, before the first appointment); it moves on from there (D-029).</summary>
    public static readonly DateTime Now = new(2026, 11, 24, 8, 20, 0);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>The appointments in a fresh tab: today's clinic days, past visits, and a few upcoming ones.</summary>
    public static List<HcAppointment> Appointments()
    {
        var today = Now.Date;
        var id = 1;
        var list = new List<HcAppointment>();

        HcAppointment Add(int patientId, string doctorId, string clinicId, int days, int hour, int minute, string reason, HcAppointmentStatus status, int bookedDaysBefore = 14)
        {
            var start = today.AddDays(days).AddHours(hour).AddMinutes(minute);
            var appointment = new HcAppointment
            {
                Id = id++, PatientId = patientId, DoctorId = doctorId, ClinicId = clinicId, Start = start, Reason = reason, Status = status,
                CreatedAt = start.AddDays(-bookedDaysBefore).Date.AddHours(17),
                CheckedInAt = status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed ? start.AddMinutes(-8) : null,
                CompletedAt = status == HcAppointmentStatus.Completed ? start.AddMinutes(25) : null,
            };
            list.Add(appointment);

            return appointment;
        }

        // Maya's history with Dr. Okoye and Dr. Rossi.
        var physical = Add(SeededPatientId, SeededDoctorId, "downtown", -35, 10, 0, "Annual physical", HcAppointmentStatus.Completed, 30);
        physical.Notes = "Annual exam. BP 128/82, BMI 27. Fasting glucose high on screening last year; continue metformin. Order CBC and lipid panel. Flu shot given.";
        physical.Summary = "Your annual physical went well. We ordered two routine blood tests (results are in Lab reports) and gave you this year's flu shot. Keep taking metformin with meals.";
        var rash = Add(SeededPatientId, "rossi", "lakeview", -14, 14, 0, "Rash on my forearm", HcAppointmentStatus.Completed, 10);
        rash.Notes = "Contact dermatitis, left forearm, likely from a new detergent. Hydrocortisone 1% twice daily for 7 days, then stop.";
        rash.Summary = "The rash is a skin reaction, probably to a new detergent. Use the hydrocortisone cream twice a day for a week, switch back to your old detergent, and book again if it isn't better in two weeks.";
        var followUp = Add(SeededPatientId, SeededDoctorId, "downtown", -7, 9, 30, "Diabetes follow-up", HcAppointmentStatus.Completed, 21);
        followUp.Notes = "Diabetes follow-up. Home readings 110–140 fasting. LDL high in October: started atorvastatin 10 mg. Order HbA1c; review results next visit.";
        followUp.Summary = "We talked about your blood sugar readings and started a cholesterol medicine. We ordered an HbA1c test; we'll go over the result at your next visit on December 1.";

        // Other patients' past visits.
        var bp = Add(2, SeededDoctorId, "downtown", -21, 8, 30, "High blood pressure", HcAppointmentStatus.Completed);
        bp.Notes = "BP 148/94 on two readings. Start lisinopril 10 mg daily. BMP to check kidney function and potassium.";
        bp.Summary = "Your blood pressure was high. We started lisinopril and checked your kidney function with a blood test.";
        Add(3, SeededDoctorId, "downtown", -27, 15, 0, "Allergy symptoms", HcAppointmentStatus.NoShow);
        var chest = Add(4, "chen", "lakeview", -12, 10, 0, "Cardiology referral", HcAppointmentStatus.Completed);
        chest.Notes = "Stable. ECG normal sinus rhythm. Repeat lipid panel.";
        chest.Summary = "Your heart tracing was normal. We repeated your cholesterol test.";
        var cancelled = Add(8, "bello", "northfield", -4, 11, 0, "Ankle sprain", HcAppointmentStatus.Cancelled, 6);
        cancelled.CancelledAt = cancelled.Start.AddDays(-1).AddHours(-3);
        cancelled.CancelReason = "Feeling better, no longer needed.";

        // Today, Tuesday: Dr. Okoye's clinic day, and a few other doctors.
        Add(2, SeededDoctorId, "downtown", 0, 8, 30, "Blood pressure check", HcAppointmentStatus.CheckedIn, 21);
        Add(3, SeededDoctorId, "downtown", 0, 9, 0, "Persistent cough", HcAppointmentStatus.Booked, 2);
        Add(4, SeededDoctorId, "downtown", 0, 9, 30, "Medication review", HcAppointmentStatus.Booked, 10);
        Add(7, SeededDoctorId, "downtown", 0, 11, 0, "Knee pain follow-up", HcAppointmentStatus.Booked, 7);
        Add(8, SeededDoctorId, "downtown", 0, 13, 30, "Sports physical", HcAppointmentStatus.Booked, 5);
        Add(5, "raman", "downtown", 0, 9, 0, "Ear pain", HcAppointmentStatus.Booked, 1);
        Add(6, "chen", "lakeview", 0, 10, 0, "Chest tightness when climbing stairs", HcAppointmentStatus.Booked, 9);

        // Upcoming.
        Add(SeededPatientId, SeededDoctorId, "downtown", 7, 10, 0, "Review my HbA1c result", HcAppointmentStatus.Booked, 7);
        Add(5, "raman", "downtown", 9, 15, 0, "Four-year checkup", HcAppointmentStatus.Booked, 20);
        Add(7, "whitaker", "northfield", 6, 9, 0, "Knee X-ray review", HcAppointmentStatus.Booked, 12);

        return list;
    }

    /// <summary>The doctors' weekly hours in a fresh tab.</summary>
    public static List<HcAvailability> Availability()
    {
        var id = 1;
        var list = new List<HcAvailability>();

        void Block(string doctorId, string clinicId, int startHour, int startMinute, int endHour, int endMinute, params DayOfWeek[] days)
        {
            foreach (var day in days)
                list.Add(new HcAvailability { Id = id++, DoctorId = doctorId, ClinicId = clinicId, Day = day, StartMinute = (startHour * 60) + startMinute, EndMinute = (endHour * 60) + endMinute });
        }

        Block(SeededDoctorId, "downtown", 8, 30, 12, 0, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);
        Block(SeededDoctorId, "downtown", 13, 0, 16, 30, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);
        Block("raman", "downtown", 9, 0, 12, 30, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);
        Block("raman", "downtown", 14, 0, 17, 0, DayOfWeek.Monday, DayOfWeek.Thursday);
        Block("chen", "lakeview", 8, 0, 12, 0, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday);
        Block("chen", "lakeview", 13, 0, 17, 0, DayOfWeek.Friday);
        Block("rossi", "lakeview", 9, 0, 16, 0, DayOfWeek.Monday, DayOfWeek.Wednesday);
        Block("rossi", "lakeview", 9, 0, 12, 0, DayOfWeek.Friday);
        Block("whitaker", "northfield", 8, 0, 12, 0, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday);
        Block("bello", "northfield", 10, 0, 13, 0, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);
        Block("bello", "northfield", 14, 0, 18, 0, DayOfWeek.Tuesday, DayOfWeek.Thursday);

        return list;
    }

    /// <summary>The bills in a fresh tab: Maya has two paid and two due.</summary>
    public static List<HcBill> Bills()
    {
        var today = Now.Date;

        return new List<HcBill>
        {
            new() { Id = 1, PatientId = SeededPatientId, AppointmentId = 1, Description = "Visit: annual physical with Dr. Samuel Okoye", AmountCents = PrimaryCareFeeCents, CreatedAt = today.AddDays(-35).AddHours(16), DueDate = today.AddDays(-5), Status = HcBillStatus.Paid, PaidAt = today.AddDays(-30).AddHours(19), CardBrand = "Visa", CardLast4 = "4242" },
            new() { Id = 2, PatientId = SeededPatientId, LabOrderId = 1, Description = "Lab: complete blood count and lipid panel", AmountCents = LabFeeCents * 2, CreatedAt = today.AddDays(-33).AddHours(9), DueDate = today.AddDays(-3), Status = HcBillStatus.Paid, PaidAt = today.AddDays(-30).AddHours(19), CardBrand = "Visa", CardLast4 = "4242" },
            new() { Id = 3, PatientId = SeededPatientId, AppointmentId = 2, Description = "Visit: dermatology with Dr. Elena Rossi", AmountCents = SpecialistFeeCents, CreatedAt = today.AddDays(-14).AddHours(15), DueDate = today.AddDays(16) },
            new() { Id = 4, PatientId = SeededPatientId, AppointmentId = 3, Description = "Visit: diabetes follow-up with Dr. Samuel Okoye", AmountCents = PrimaryCareFeeCents, CreatedAt = today.AddDays(-7).AddHours(10), DueDate = today.AddDays(23) },
        };
    }

    /// <summary>The clinic locations.</summary>
    public static IReadOnlyList<HcClinic> Clinics() => new List<HcClinic>
    {
        new("downtown", "Cedarline Downtown", "410 Market Street", "(555) 010-2200", "Mon–Fri 8:00–17:00", 47.6097, -122.3383),
        new("lakeview", "Cedarline Lakeview", "2200 Lakeview Avenue", "(555) 010-2300", "Mon–Fri 8:00–17:30", 47.6390, -122.3260),
        new("northfield", "Cedarline Northfield", "88 Northfield Road", "(555) 010-2400", "Mon–Fri 8:00–18:30", 47.7000, -122.3250),
    };

    /// <summary>The demo accounts: a patient, a doctor, and the front desk.</summary>
    public static IReadOnlyList<ShowcaseDemoAccount> DemoAccounts() => new List<ShowcaseDemoAccount>
    {
        new(SeededPatientAccountId, PatientRole, "Maya Thompson", "maya.thompson@example.com", "care-demo-26", "MT"),
        new(SeededDoctorId, DoctorRole, "Dr. Samuel Okoye", "samuel.okoye@example.com", "care-doc-26", "SO"),
        new(SeededFrontDeskId, FrontDeskRole, "Lena Fischer", "lena.fischer@example.com", "care-desk-26", "LF"),
    };

    /// <summary>The doctors.</summary>
    public static IReadOnlyList<HcDoctor> Doctors() => new List<HcDoctor>
    {
        new() { Id = SeededDoctorId, AccountId = SeededDoctorId, Name = "Dr. Samuel Okoye", Initials = "SO", Specialty = "Family medicine", ClinicId = "downtown", Languages = new[] { "English", "Yoruba" },
            Bio = "Dr. Okoye cares for adults and families, with a focus on diabetes, blood pressure, and preventive care. He has practiced at Cedarline for twelve years." },
        new() { Id = "raman", Name = "Dr. Priya Raman", Initials = "PR", Specialty = "Pediatrics", ClinicId = "downtown", Languages = new[] { "English", "Tamil", "Hindi" },
            Bio = "Dr. Raman sees babies, children, and teenagers, from newborn checkups to sports physicals." },
        new() { Id = "chen", Name = "Dr. Michael Chen", Initials = "MC", Specialty = "Cardiology", ClinicId = "lakeview", Languages = new[] { "English", "Mandarin" },
            Bio = "Dr. Chen treats heart rhythm problems, chest pain, and high cholesterol, and runs the Lakeview heart clinic." },
        new() { Id = "rossi", Name = "Dr. Elena Rossi", Initials = "ER", Specialty = "Dermatology", ClinicId = "lakeview", Languages = new[] { "English", "Italian" },
            Bio = "Dr. Rossi treats skin, hair, and nail conditions, and does skin cancer screening." },
        new() { Id = "whitaker", Name = "Dr. James Whitaker", Initials = "JW", Specialty = "Orthopedics", ClinicId = "northfield", Languages = new[] { "English" }, AcceptingNewPatients = false,
            Bio = "Dr. Whitaker treats joint and sports injuries, with a focus on knees and shoulders. He is not taking new patients right now." },
        new() { Id = "bello", Name = "Dr. Aisha Bello", Initials = "AB", Specialty = "Family medicine", ClinicId = "northfield", Languages = new[] { "English", "French", "Hausa" },
            Bio = "Dr. Bello cares for patients of all ages, including women's health and travel vaccinations, with late hours on Tuesdays and Thursdays." },
    };

    /// <summary>The lab tests doctors can order.</summary>
    public static IReadOnlyList<HcLabPanel> LabPanels() => new List<HcLabPanel>
    {
        new("cbc", "Complete blood count", "Red and white blood cells and platelets.", new[]
        {
            new HcAnalyte("White blood cells", "×10⁹/L", 4.0, 11.0), new HcAnalyte("Hemoglobin", "g/dL", 12.0, 15.5), new HcAnalyte("Platelets", "×10⁹/L", 150, 450),
        }),
        new("lipid", "Lipid panel", "Cholesterol and triglycerides.", new[]
        {
            new HcAnalyte("Total cholesterol", "mg/dL", 0, 200), new HcAnalyte("LDL cholesterol", "mg/dL", 0, 100), new HcAnalyte("HDL cholesterol", "mg/dL", 40, 100),
            new HcAnalyte("Triglycerides", "mg/dL", 0, 150),
        }),
        new("a1c", "Hemoglobin A1c", "Average blood sugar over the last three months.", new[] { new HcAnalyte("HbA1c", "%", 4.0, 5.6) }),
        new("bmp", "Basic metabolic panel", "Blood sugar, salts, and kidney function.", new[]
        {
            new HcAnalyte("Glucose", "mg/dL", 70, 99), new HcAnalyte("Sodium", "mmol/L", 135, 145), new HcAnalyte("Potassium", "mmol/L", 3.5, 5.1), new HcAnalyte("Creatinine", "mg/dL", 0.6, 1.2),
        }),
        new("tsh", "Thyroid stimulating hormone", "How well the thyroid is working.", new[] { new HcAnalyte("TSH", "mIU/L", 0.4, 4.0) }),
    };

    /// <summary>The lab orders in a fresh tab: Maya has two released reports and one waiting for results.</summary>
    public static List<HcLabOrder> LabOrders()
    {
        var today = Now.Date;

        return new List<HcLabOrder>
        {
            new() { Id = 1, PatientId = SeededPatientId, DoctorId = SeededDoctorId, AppointmentId = 1, PanelId = "cbc", OrderedAt = today.AddDays(-35).AddHours(10.5), Status = HcLabStatus.Released,
                Values = new() { 6.2, 13.4, 248 }, ResultedAt = today.AddDays(-34).AddHours(14), ReleasedAt = today.AddDays(-33).AddHours(9), Comment = "All normal." },
            new() { Id = 2, PatientId = SeededPatientId, DoctorId = SeededDoctorId, AppointmentId = 1, PanelId = "lipid", OrderedAt = today.AddDays(-35).AddHours(10.5), Status = HcLabStatus.Released,
                Values = new() { 224, 142, 46, 180 }, ResultedAt = today.AddDays(-34).AddHours(14), ReleasedAt = today.AddDays(-33).AddHours(9),
                Comment = "Your LDL (\"bad\") cholesterol and triglycerides are high. We'll talk about a cholesterol medicine at your follow-up." },
            new() { Id = 3, PatientId = SeededPatientId, DoctorId = SeededDoctorId, AppointmentId = 3, PanelId = "a1c", OrderedAt = today.AddDays(-7).AddHours(9.9), Status = HcLabStatus.Ordered },
            new() { Id = 4, PatientId = 2, DoctorId = SeededDoctorId, AppointmentId = 4, PanelId = "bmp", OrderedAt = today.AddDays(-21).AddHours(9), Status = HcLabStatus.Released,
                Values = new() { 96, 140, 4.4, 0.9 }, ResultedAt = today.AddDays(-20).AddHours(15), ReleasedAt = today.AddDays(-20).AddHours(16), Comment = "Kidney function and potassium are normal." },
            new() { Id = 5, PatientId = 4, DoctorId = "chen", AppointmentId = 6, PanelId = "lipid", OrderedAt = today.AddDays(-12).AddHours(10.5), Status = HcLabStatus.Resulted,
                Values = new() { 188, 96, 52, 120 }, ResultedAt = today.AddDays(-10).AddHours(11) },
        };
    }

    /// <summary>The patients in a fresh tab: Maya (the demo account) and seven who can't sign in.</summary>
    public static List<HcPatient> Patients() => new()
    {
        new() { Id = SeededPatientId, AccountId = SeededPatientAccountId, Name = "Maya Thompson", DateOfBirth = new DateTime(1988, 4, 12), Email = "maya.thompson@example.com", Phone = "(555) 014-2290",
            Allergies = { "Penicillin (hives)" }, Conditions = { "Type 2 diabetes", "High cholesterol" },
            Immunizations = { new HcImmunization("Influenza", Now.Date.AddDays(-35)), new HcImmunization("COVID-19 booster", new DateTime(2025, 11, 2)), new HcImmunization("Tdap", new DateTime(2022, 3, 14)) } },
        new() { Id = 2, Name = "Daniel Brooks", DateOfBirth = new DateTime(1975, 9, 3), Email = "daniel.brooks@example.com", Phone = "(555) 014-3301", Conditions = { "High blood pressure" } },
        new() { Id = 3, Name = "Sofia Alvarez", DateOfBirth = new DateTime(1992, 1, 27), Email = "sofia.alvarez@example.com", Phone = "(555) 014-3302", Allergies = { "Pollen" } },
        new() { Id = 4, Name = "Henry Park", DateOfBirth = new DateTime(1960, 6, 18), Email = "henry.park@example.com", Phone = "(555) 014-3303", Conditions = { "High cholesterol", "Atrial fibrillation" } },
        new() { Id = 5, Name = "Grace Liu", DateOfBirth = new DateTime(2022, 11, 30), Email = "liu.family@example.com", Phone = "(555) 014-3304" },
        new() { Id = 6, Name = "Omar Farouk", DateOfBirth = new DateTime(1983, 2, 9), Email = "omar.farouk@example.com", Phone = "(555) 014-3305", Allergies = { "Sulfa drugs" } },
        new() { Id = 7, Name = "Ruth Bennett", DateOfBirth = new DateTime(1948, 12, 1), Email = "ruth.bennett@example.com", Phone = "(555) 014-3306", Conditions = { "Osteoarthritis of the knee" } },
        new() { Id = 8, Name = "Lucas Meyer", DateOfBirth = new DateTime(2001, 7, 22), Email = "lucas.meyer@example.com", Phone = "(555) 014-3307" },
    };

    /// <summary>The prescriptions in a fresh tab.</summary>
    public static List<HcPrescription> Prescriptions()
    {
        var today = Now.Date;

        return new List<HcPrescription>
        {
            new() { Id = 1, PatientId = SeededPatientId, DoctorId = SeededDoctorId, Medication = "Metformin", Dose = "500 mg tablet", Directions = "Take one tablet twice a day with meals.", Quantity = 60, Refills = 0, PrescribedAt = today.AddDays(-35).AddHours(10.4), Status = HcPrescriptionStatus.Active },
            new() { Id = 2, PatientId = SeededPatientId, DoctorId = SeededDoctorId, Medication = "Atorvastatin", Dose = "10 mg tablet", Directions = "Take one tablet at bedtime.", Quantity = 30, Refills = 3, PrescribedAt = today.AddDays(-7).AddHours(9.9), Status = HcPrescriptionStatus.Active },
            new() { Id = 3, PatientId = SeededPatientId, DoctorId = "rossi", Medication = "Hydrocortisone", Dose = "1% cream", Directions = "Apply a thin layer to the rash twice a day for 7 days.", Quantity = 1, Refills = 0, PrescribedAt = today.AddDays(-14).AddHours(14.4), Status = HcPrescriptionStatus.Stopped },
            new() { Id = 4, PatientId = 2, DoctorId = SeededDoctorId, Medication = "Lisinopril", Dose = "10 mg tablet", Directions = "Take one tablet every morning.", Quantity = 30, Refills = 5, PrescribedAt = today.AddDays(-21).AddHours(8.9), Status = HcPrescriptionStatus.Active },
        };
    }

    /// <summary>The roles of the site, with their start pages.</summary>
    public static IReadOnlyList<ShowcaseRole> Roles() => new List<ShowcaseRole>
    {
        new(PatientRole, "Patient", "/showcase/healthcare/appointments", "/showcase/healthcare/profile", "Books and manages appointments, and sees records, lab reports, prescriptions, and bills."),
        new(DoctorRole, "Doctor", "/showcase/healthcare/staff/schedule", "/showcase/healthcare/staff/account", "Runs their schedule: visit notes, patient charts, lab results, and prescriptions."),
        new(FrontDeskRole, "Front desk", "/showcase/healthcare/staff/calendar", "/showcase/healthcare/staff/account", "Runs the clinics' calendar, checks patients in, and keeps doctors' hours and time off."),
    };

    /// <summary>Doctors' time off in a fresh tab.</summary>
    public static List<HcTimeOff> TimeOff() => new()
    {
        new() { Id = 1, DoctorId = "rossi", From = Now.Date.AddDays(13), To = Now.Date.AddDays(15), Reason = "Dermatology conference" },
        new() { Id = 2, DoctorId = "chen", From = Now.Date.AddDays(30), To = Now.Date.AddDays(34), Reason = "Holiday" },
    };

    #endregion

    #endregion
}
