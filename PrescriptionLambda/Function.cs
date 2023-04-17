using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json.Serialization;
using System.Data.SqlClient;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PrescriptionLambda;

public class Function
{
    //private static string secretResponse ;
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public  async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        string jsonresponse = string.Empty;
        try
        {

            List<Prescription> prescription = new List<Prescription>();
            string id = string.Empty;
            input.PathParameters?.TryGetValue("id", out id);
            string query = $"select * from Prescriptions where Id = {id}";
            string connectionString = "Server=clinic-mssql.cyhbv4dqbk22.us-east-1.rds.amazonaws.com,1433;User Id=admin;Password=#$Suadmin#$;Trusted_Connection=false; MultipleActiveResultSets=true;database=clinicDb;TrustServerCertificate=True";
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    Console.WriteLine("Try connecting to RDS");
                    conn.Open();
                    Console.WriteLine("connection successfull!");
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        prescription.Add(new Prescription
                        {
                            Id = Convert.ToInt32(rdr[0]),
                            PatientName = rdr[1].ToString(),
                            DoctorId = Convert.ToInt32(rdr[2]),
                            AppointmentId = Convert.ToInt32(rdr[3]),
                            CreatedDate = Convert.ToDateTime(rdr[4]),
                        });
                    }
                }
            }

            jsonresponse = JsonConvert.SerializeObject(prescription);

            return new APIGatewayProxyResponse
            {

                StatusCode = 200,
                Body = jsonresponse,
                Headers = { }
            };
        }
        catch(Exception ex)
        {
            jsonresponse = ex.Message;
            return new APIGatewayProxyResponse
            {

                StatusCode = 502,
                Body = jsonresponse,
                Headers = { }
            };

        }

    }
}
class Prescription
{
    public int Id { get; set; }
    public string PatientName { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentId { get; set; }
    public DateTime CreatedDate { get; set; }
}