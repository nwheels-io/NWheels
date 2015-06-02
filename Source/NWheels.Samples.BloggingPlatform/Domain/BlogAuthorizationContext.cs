#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Security;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public class BlogAuthorizationContext : AuthorizationContextBase<BlogUserRole>
    {
        private readonly ILogger _logger;
        private readonly IBlogUserAuthorizationEntity[] _authorizations;
        private readonly HashSet<BlogUserRole> _userRoles;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlogAuthorizationContext(IBlogUserAccountEntity user, IBlogEntity blog, Auto<ILogger> logger)
        {
            _logger = logger.Instance;
            _authorizations = blog.Authorizations.Where(au => au.User.Equals(user)).ToArray();
            _userRoles = new HashSet<BlogUserRole>(_authorizations.Select(au => au.Role.Role));

            this.User = user;
            this.Blog = blog;

            if ( !_authorizations.Any() )
            {
                throw _logger.UserHasNoAuthorizationForSpecifiedBlog(user, blog);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IBlogEntity Blog { get; private set; }
        public IBlogUserAccountEntity User { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of AuthorizationContextBase<BlogUserRole>

        protected override bool OnAuthorizeOperation(IEnumerable<BlogUserRole> authorizedRoles)
        {
            return _userRoles.Overlaps(authorizedRoles);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void BuildEntityAccessRules(IEntityAccessControlBuilder access)
        {
            if ( !_userRoles.Contains(BlogUserRole.Admin) )
            {
                access.ToEntity<IBlogUserAccountEntity>().IsFiltered(user => user == this.User);
            }

            access.ToEntity<IBlogEntity>(blog => blog.Authorizations.All(au => au.User != this.User)).IsReadOnly();
            access.ToEntity<IPostEntity>(post => post.Author != this.User).IsReadOnly();
            access.ToEntity<IArticleEntity>(article => article.Author != this.User).IsReadOnly();
            access.ToEntity<IReplyEntity>(reply => reply.Author != this.User).IsReadOnly();
        }

        #endregion
    }
}

#endif