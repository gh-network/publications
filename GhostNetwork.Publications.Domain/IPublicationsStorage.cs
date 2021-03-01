using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GhostNetwork.Publications
{
    public interface IPublicationsStorage
    {
        Task<Publication> FindOneByIdAsync(string id);

        Task<string> InsertOneAsync(Publication publication);

        Task<(IEnumerable<Publication>, long)> FindManyAsync(int skip, int take, IEnumerable<string> tags, Ordering order);

        Task UpdateOneAsync(Publication publication);

        Task DeleteOneAsync(string id);

        Task<(IEnumerable<Publication>, long)> FindManyByAuthorAsync(int skip, int take, Guid authorId, Ordering order);

        Task UpdateImagesUrlAsync(string id, string imagesUrl);

        Task DeleteImagesUrlAsync(string id);
    }
}
