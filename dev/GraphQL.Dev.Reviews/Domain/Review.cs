namespace GraphQL.Dev.Reviews.Domain;

public class Review
{
    public string AuthorID { get; set; }

    public string Body { get; set; }
    public string ID { get; set; }

    public Product Product { get; set; }
}