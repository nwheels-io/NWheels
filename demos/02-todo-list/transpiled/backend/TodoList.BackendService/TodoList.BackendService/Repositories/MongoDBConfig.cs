namespace TodoList.BackendService.Repositories
{
    public class MongoDBConfig
    {
        public string Host;
        public string Database;
        public CredentialConfig Credential;

        public class CredentialConfig
        {
            public string Database;
            public string UserName;
            public string Password;
        }
    }
}