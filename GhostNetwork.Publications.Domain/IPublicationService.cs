using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain;
using Domain.Validation;
using GhostNetwork.Publications.AzureBlobStorage;
using GhostNetwork.Publications.Comments;

namespace GhostNetwork.Publications
{
    public interface IPublicationService
    {
        Task<Publication> GetByIdAsync(string id);

        Task<(IEnumerable<Publication>, long)> SearchAsync(int skip, int take, IEnumerable<string> tags, Ordering order);

        Task<(DomainResult, string)> CreateAsync(string text, UserInfo author);

        Task<DomainResult> UpdateAsync(string id, string text);

        Task DeleteAsync(string id);

        Task<(IEnumerable<Publication>, long)> SearchByAuthor(int skip, int take, Guid authorId, Ordering order);

        Task<DomainResult> UpdateImagesAsync(string id, Stream stream, string extension);

        Task<DomainResult> DeleteImagesAsync(string id);
    }

    public class PublicationService : IPublicationService
    {
        private readonly IValidator<PublicationContext> validator;
        private readonly IPublicationsStorage publicationStorage;
        private readonly ICommentsStorage commentStorage;
        private readonly IHashTagsFetcher hashTagsFetcher;
        private readonly IImagesStorage imagesStorage;

        public PublicationService(
            IValidator<PublicationContext> validator,
            IPublicationsStorage publicationStorage,
            ICommentsStorage commentStorage,
            IHashTagsFetcher hashTagsFetcher,
            IImagesStorage imagesStorage)
        {
            this.validator = validator;
            this.publicationStorage = publicationStorage;
            this.commentStorage = commentStorage;
            this.hashTagsFetcher = hashTagsFetcher;
            this.imagesStorage = imagesStorage;
        }

        public async Task<Publication> GetByIdAsync(string id)
        {
            return await publicationStorage.FindOneByIdAsync(id);
        }

        public async Task<(IEnumerable<Publication>, long)> SearchAsync(int skip, int take, IEnumerable<string> tags, Ordering order)
        {
            return await publicationStorage.FindManyAsync(skip, take, tags, order);
        }

        public async Task<(DomainResult, string)> CreateAsync(string text, UserInfo author)
        {
            var content = new PublicationContext(text);
            var result = await validator.ValidateAsync(content);

            if (!result.Successed)
            {
                return (result, null);
            }

            var publication = Publication.New(text, author, hashTagsFetcher.Fetch);
            var id = await publicationStorage.InsertOneAsync(publication);

            return (result, id);
        }

        public async Task<DomainResult> UpdateAsync(string id, string text)
        {
            var content = new PublicationContext(text);
            var result = await validator.ValidateAsync(content);

            if (!result.Successed)
            {
                return result;
            }

            var publication = await publicationStorage.FindOneByIdAsync(id);

            publication.Update(text, hashTagsFetcher.Fetch);

            await publicationStorage.UpdateOneAsync(publication);

            return DomainResult.Success();
        }

        public async Task DeleteAsync(string id)
        {
            var publication = await publicationStorage.FindOneByIdAsync(id);
            if (publication.ImagesUrl != null)
            {
                await imagesStorage.DeleteImagesAsync(publication.ImagesUrl);
            }

            await commentStorage.DeleteByPublicationAsync(id);
            await publicationStorage.DeleteOneAsync(id);
        }

        public async Task<(IEnumerable<Publication>, long)> SearchByAuthor(int skip, int take, Guid authorId, Ordering order)
        {
            return await publicationStorage.FindManyByAuthorAsync(skip, take, authorId, order);
        }

        public async Task<DomainResult> UpdateImagesAsync(string id, Stream stream, string extension)
        {
            var publication = await publicationStorage.FindOneByIdAsync(id);
            if (publication == null)
            {
                return DomainResult.Error("Publication not found.");
            }

            var fileName = Guid.NewGuid() + extension;
            stream.Position = 0;

            var imagesUrl = await imagesStorage.UploadImagesAsync(stream, fileName);
            if (publication.ImagesUrl != null)
            {
                return DomainResult.Error("Images is exist");
            }

            await publicationStorage.UpdateImagesUrlAsync(id, imagesUrl);
            return DomainResult.Success();
        }

        public async Task<DomainResult> DeleteImagesAsync(string id)
        {
            var publication = await publicationStorage.FindOneByIdAsync(id);

            await imagesStorage.DeleteImagesAsync(publication.ImagesUrl);

            await publicationStorage.DeleteImagesUrlAsync(id);

            return DomainResult.Success();
        }
    }
}
