using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Security;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public class BlogAuthorizationContext : IAuthorizationContext<BlogUserRole>
    {
        private readonly ILogger _logger;
        private readonly IBlogUserAuthorizationEntity[] _authorizations;
        private readonly HashSet<BlogUserRole> _authorizedRoles;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlogAuthorizationContext(IBlogUserAccountEntity user, IBlogEntity blog, Auto<ILogger> logger)
        {
            _logger = logger.Instance;
            _authorizations = blog.Authorizations.Where(au => au.User.Equals(user)).ToArray();
            _authorizedRoles = new HashSet<BlogUserRole>(_authorizations.Select(au => au.Role.Role));

            this.User = user;
            this.Blog = blog;

            if ( !_authorizations.Any() )
            {
                throw _logger.UserHasNoAuthorizationForSpecifiedBlog(user, blog);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Authorize(IEnumerable<BlogUserRole> allowedRoles)
        {
            return _authorizedRoles.Overlaps(allowedRoles);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IBlogEntity Blog { get; private set; }
        public IBlogUserAccountEntity User { get; private set; }
    }
}
