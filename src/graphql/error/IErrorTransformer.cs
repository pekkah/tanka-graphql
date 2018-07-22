using System;
using System.Collections.Generic;

namespace fugu.graphql.error
{
    public interface IErrorTransformer
    {
        IEnumerable<Error> Transfrom(Exception exception);
    }
}