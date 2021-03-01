using System;
using System.Collections.Generic;

namespace GhostNetwork.Publications
{
    public class Publication
    {
        public Publication(string id, string content, IEnumerable<string> tags, UserInfo author, DateTimeOffset createdOn, DateTimeOffset updatedOn, string imagesUrl)
        {
            Id = id;
            Content = content;
            Tags = tags;
            Author = author;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }

        public string Id { get; }

        public string Content { get; private set; }

        public IEnumerable<string> Tags { get; private set; }

        public UserInfo Author { get; }

        public DateTimeOffset CreatedOn { get; }

        public DateTimeOffset UpdatedOn { get; private set; }

        public string ImagesUrl { get; private set; }

        public bool IsUpdated => CreatedOn != UpdatedOn;

        public static Publication New(string content, UserInfo author, Func<string, IEnumerable<string>> tagsFetcher)
        {
            var now = DateTimeOffset.UtcNow;

            return new Publication(default, content, tagsFetcher(content), author, now, now, null);
        }

        public Publication Update(string content, Func<string, IEnumerable<string>> tagsFetcher)
        {
            Content = content;
            UpdatedOn = DateTimeOffset.UtcNow;
            Tags = tagsFetcher(content);

            return this;
        }
    }
}