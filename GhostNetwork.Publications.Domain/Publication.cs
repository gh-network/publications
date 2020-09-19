using System;
using System.Collections.Generic;

namespace GhostNetwork.Publications.Domain
{
    public class Publication
    {
        public Publication(string id, string content, DateTimeOffset createdOn, IEnumerable<string> tags, DateTimeOffset? updatedOn = null)
        {
            Id = id;
            Content = content;
            CreatedOn = createdOn;
            Tags = tags;
            UpdatedOn = updatedOn;
        }

        public string Id { get; }

        public string Content { get; }

        public DateTimeOffset CreatedOn { get; }

        public DateTimeOffset? UpdatedOn { get; }

        public IEnumerable<string> Tags { get; }
    }
}