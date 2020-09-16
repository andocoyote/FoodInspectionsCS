namespace HttpClientTest.Model
{
    // This models the data retrieved from the inspection API and contains all of the available fields
    public class InspectionData
    {
        public string Name { get; set; } = null;
        public string Program_Identifier { get; set; } = null;
        public string Inspection_Date { get; set; } = null;
        public string Description { get; set; } = null;
        public string Address { get; set; } = null;
        public string City { get; set; } = null;
        public string Zip_Code { get; set; } = null;
        public string Phone { get; set; } = null;
        public string Longitude { get; set; } = null;
        public string Latitude { get; set; } = null;
        public string Inspection_Business_Name { get; set; } = null;
        public string Inspection_Type { get; set; } = null;
        public string Inspection_Score { get; set; } = null;
        public string Inspection_Result { get; set; } = null;
        public string Inspection_Closed_Business { get; set; } = null;
        public string Violation_Points { get; set; } = null;
        public string Business_Id { get; set; } = null;
        public string Inspection_Serial_Num { get; set; } = null;
        public string Grade { get; set; } = null;
    }
}
