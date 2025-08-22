using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using RevueCrafters_exam_project.Models;
using RevueCragters.Models;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;





namespace RevueCrafters_exam_project
{
    [TestFixture]
    public class RevueCraftersTests
    {
        private RestClient client;
        private static string? createdRevueId;
        private static string baseURL = "https://d2925tksfvgq8c.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("evka86@evka.bg", "123456");


            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }


        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }
        [Order(1)]
        [Test]
        public void CreateRevue_ShouldReturnOK()
        { 
            var revue = new
            {
                title = "My new Revue",
                url = "",
                description = "This is my first revue "
            };
            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(revue);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
          
          Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
           


        }
        [Order(2)]
        [Test]
        public void GetAllRevues_ShouldPresentAll()
        {
            var request = new RestRequest("/api/Revue/All", Method.Get);
            var response = client.Execute(request);
            var responceItems = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
           
            Assert.That(responceItems,Is.Not.Empty);

            Assert.That(responceItems.Count, Is.GreaterThan(0), "Expected at least one revue to be present");
   createdRevueId = JsonSerializer.Deserialize<JsonElement>(response.Content)[0].GetProperty("id").GetString();
            Assert.IsNotNull(createdRevueId, "Created Revue ID should not be null");



        }
        [Order(3)]
        [Test]
        public void EditLastRevue_ShouldReturnOK()
        {
            var changedRevue = new RevueDTO
            {
                title = "My changed Revue",
                url = "",
                description = "This is my changed revue "
            };
            var request = new RestRequest("/api/Revue/Edit/", Method.Put);
            request.AddQueryParameter("revueId", createdRevueId);
            request.AddJsonBody(changedRevue);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
           
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));

        }
        [Order(4)]
        [Test]
        public void DeleteTheEditedRevue_ShouldReturnDeleted()
        { var request = new RestRequest("/api/Revue/Delete/", Method.Delete);
            request.AddQueryParameter("revueId", createdRevueId);
            var response = client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
           
            Assert.That(deleteResponse.Msg, Is.EqualTo("The revue is deleted!"));
        }
        [Order(5)]
        [Test]
        public void CreatRevueWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var revue = new
            {
                title = "",
                url = "",
                description = ""
            };
            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(revue);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request");
        }
        [Order(6)]
        [Test]
        public void EditNonExistingRevue_ShouldReturnBadRequest()
        { 
            var fakeRevueId = "00000000-0000-0000-0000-000000000000";   
            var changedRevue = new RevueDTO
            {
                title = "My changed Revue",
                url = "",
                description = "This is my changed revue "
            };
            var request = new RestRequest("/api/Revue/Edit/", Method.Put);
            request.AddQueryParameter("revueId", createdRevueId);
            request.AddJsonBody(changedRevue);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(editResponse.Msg, Is.EqualTo("There is no such revue!"));
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistingRevue_ShouldReturnBadRequest()
        {
            var fakeRevueId = "00000000-0000-0000-0000-000000000000";
            var request = new RestRequest("/api/Revue/Delete/", Method.Delete);
            request.AddQueryParameter("revueId", fakeRevueId);
            var response = client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request");
            Assert.That(deleteResponse.Msg, Is.EqualTo("There is no such revue!"));
        }
        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();

        }
    }
}
