namespace UnobtrusiveMVCTechniques.Repositories
{
    public class User
    {
        public string UserName { get; set; }
    }

    public interface IUserRepository
    {
        User GetUserByUserName(string userName);
    }

    public class UserRepository : IUserRepository
    {
        public User GetUserByUserName(string userName)
        {
            // In reality this would obviously check a datasource of some sort
            if (userName == "dummy")
            {
                return new User {UserName = userName};
            }
            return null;
        }
    }
}