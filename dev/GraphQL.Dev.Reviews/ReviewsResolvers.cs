using System.Linq;
using System.Threading.Tasks;
using GraphQL.Dev.Reviews.Domain;
using Tanka.GraphQL.ValueResolution;

namespace GraphQL.Dev.Reviews;

public class ReviewsResolvers : ResolversMap
{
    public ReviewsResolvers()
    {
        this["User"] = new()
        {
            { "id", context => context.ResolveAsPropertyOf<User>(u => u.ID) },
            { "username", UserUsername },
            { "reviews", UserReviews }
        };

        this["Review"] = new()
        {
            { "id", context => context.ResolveAsPropertyOf<Review>(r => r.ID) },
            { "body", context => context.ResolveAsPropertyOf<Review>(r => r.Body) },
            { "author", ReviewAuthor },
            { "product", context => context.ResolveAsPropertyOf<Review>(r => r.Product) }
        };

        this["Product"] = new()
        {
            { "upc", context => context.ResolveAsPropertyOf<Product>(p => p.Upc) },
            { "reviews", ProductReviews }
        };
    }

    private static ValueTask UserUsername(ResolverContext context)
    {
        var user = (User)context.ObjectValue;
        return context.ResolveAs(user.Username);
    }

    private static ValueTask ProductReviews(ResolverContext context)
    {
        var product = (Product)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.Product.Upc == product.Upc)
            .Select(p => p.Value);

        return context.ResolveAs(reviews);
    }

    private static ValueTask ReviewAuthor(ResolverContext context)
    {
        var review = (Review)context.ObjectValue;

        return context.ResolveAs(new User
        {
            ID = review.AuthorID,
            Username = Db.Usernames[review.AuthorID]
        });
    }

    private static ValueTask UserReviews(ResolverContext context)
    {
        var user = (User)context.ObjectValue;
        var reviews = Db.Reviews
            .Where(r => r.Value.AuthorID == user.ID)
            .Select(r => r.Value);

        return context.ResolveAs(reviews);
    }
}