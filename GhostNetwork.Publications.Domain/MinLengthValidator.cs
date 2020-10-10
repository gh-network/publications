using System;
using System.Threading.Tasks;
using GhostNetwork.Publications.Comments;

namespace GhostNetwork.Publications
{
    public class MinLengthValidator : IValidator<PublicationContext>,
        IValidator<CommentContext>
    {
        private readonly int minLength;

        public MinLengthValidator(int minLength)
        {
            if (minLength < 0)
            {
                throw new ArgumentException(nameof(minLength));
            }

            this.minLength = minLength;
        }

        public Task<DomainResult> ValidateAsync(PublicationContext context)
        {
            return Task.FromResult(Validate(context.Content));
        }

        public Task<DomainResult> ValidateAsync(CommentContext context)
        {
            return Task.FromResult(Validate(context.Content));
        }

        private DomainResult Validate(string content)
        {
            return content.Length < minLength
                ? DomainResult.Error($"Content length is less than {minLength} characters")
                : DomainResult.Successed();
        }
    }
}