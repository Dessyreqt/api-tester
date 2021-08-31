namespace ApiTester
{
    using System;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using Shouldly;
    using Xunit;

    public class BaseApiTest
    {
        [Fact]
        public async Task RunHealthcheck()
        {
            var json = await ApiSettings.BaseUrl.AppendPathSegment("healthcheck").GetAsync().ReceiveJson();
            string status = json.status;
            status.ShouldBe("Healthy");
        }

        [Fact]
        public async Task RegisterUser()
        {
            var email = $"{Guid.NewGuid():N}@dscarroll.com";
            var password = "testpassword";

            await CreateUser(email, password);
            await TestDuplicateSend(email, password);
        }

        private static async Task CreateUser(string email, string password)
        {
            var request = new { email, password, confirmPassword = password };

            var response = await ApiSettings.BaseUrl.AppendPathSegments("api", "users", "actions", "register").PostJsonAsync(request);
 
            response.StatusCode.ShouldBe(200);
        }

        private static async Task TestDuplicateSend(string email, string password)
        {
            var request = new { email, password, confirmPassword = password };

            try
            {
                await ApiSettings.BaseUrl.AppendPathSegments("api", "users", "actions", "register").PostJsonAsync(request);
            }
            catch (FlurlHttpException response)
            {
                response.StatusCode.ShouldBe(400);
                var json = await response.GetResponseJsonAsync();
                string message = json.errors[0].message;
                message.ShouldBe($"User name '{email}' is already taken.");
            }
        }
    }
}
