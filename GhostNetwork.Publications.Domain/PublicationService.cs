﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GhostNetwork.Publications.Domain.ContentValidation;

namespace GhostNetwork.Publications.Domain
{
    public class PublicationService : IPublicationService
    {
        private readonly IPublicationValidator validator;
        private readonly PublicationBuilder publicationBuilder;
        private readonly IPublicationStorage publicationStorage;

        public PublicationService(IPublicationValidator validator,
            PublicationBuilder publicationBuilder,
            IPublicationStorage publicationStorage)
        {
            this.validator = validator;
            this.publicationBuilder = publicationBuilder;
            this.publicationStorage = publicationStorage;
        }

        public async Task<(DomainResult, string)> CreateAsync(string text)
        {
            var content = new PublicationContext(text);
            var result = await validator.ValidateAsync(content);

            if (!result.Success)
            {
                return (result, null);
            }

            var publication = publicationBuilder.Build(text);
            var id = await publicationStorage.InsertOneAsync(publication);

            return (result, id);
        }

        public async Task<Publication> FindOneByIdAsync(string id)
        {
            if (id != null)
            {
                var publication = await publicationStorage.FindOneByIdAsync(id);
                return publication;
            }

            return null;
        }

        public async Task<IEnumerable<Publication>> FindManyAsync(int skip, int take, IEnumerable<string> tags)
        {
            var publications = await publicationStorage.FindManyAsync(skip, take, tags);
            return publications;
        }

        public async Task<DomainResult> UpdateOneAsync(string id, string text)
        {
            var content = new PublicationContext(text);
            var result = await validator.ValidateAsync(content);

            if (!result.Success)
            {
                return result;
            }

            var publications = publicationBuilder.Build(text);
            await publicationStorage.UpdateOneAsync(id, publications);

            return DomainResult.Successed();
        }
    }
}
