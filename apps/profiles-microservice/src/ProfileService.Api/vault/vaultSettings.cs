namespace ProfileService.Api.vault
{
    public class VaultSettings
    {
        public string Address { get; set; } = "http://localhost:8200";
        public string Token { get; set; } = "";
        public string MountPoint { get; set; } = "secret";
        public string RabbitMqSecretPath { get; set; } = "profile-service-mq";
    }
}
