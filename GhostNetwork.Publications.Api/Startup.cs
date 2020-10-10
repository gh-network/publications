using System;
using System.Collections.Generic;
using System.Linq;
using GhostNetwork.Publications.Api.Helpers.OpenApi;
using GhostNetwork.Publications.Comments;
using GhostNetwork.Publications.MongoDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace GhostNetwork.Publications.Api
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GhostNetwork/Publications API",
                    Version = "1.0.0"
                });

                options.OperationFilter<OperationIdFilter>();
            });

            services.AddScoped(provider =>
            {
                var client = new MongoClient($"mongodb://{configuration["MONGO_ADDRESS"]}/gpublications");
                return new MongoDbContext(client.GetDatabase("gpublications"));
            });

            services.AddScoped<IHashTagsFetcher, DefaultHashTagsFetcher>();
            services.AddScoped(provider => new ForbiddenWordsValidator(Enumerable.Empty<string>()));
            services.AddScoped<PublicationBuilder>();

            services.AddScoped<IPublicationsStorage, MongoPublicationStorage>();
            services.AddScoped<IPublicationService, PublicationService>();
            services.AddScoped(BuildPublicationValidator);

            services.AddScoped<ICommentsStorage, MongoCommentsStorage>();
            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped(BuildCommentValidator);

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseSwagger()
                    .UseSwaggerUI(config =>
                    {
                        config.SwaggerEndpoint("/swagger/v1/swagger.json", "Relations.API V1");
                    });

                app.UseCors(builder => builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private IValidator<PublicationContext> BuildPublicationValidator(IServiceProvider provider)
        {
            var validators = new List<IValidator<PublicationContext>>
            {
                provider.GetService<ForbiddenWordsValidator>(),
                new MaxLengthValidator(configuration.GetValue<int?>("PUBLICATION_CONTENT_MAX_LENGTH") ?? 5000)
            };

            var minLength = configuration.GetValue<int?>("PUBLICATION_CONTENT_MIN_LENGTH");
            if (minLength.HasValue)
            {
                validators.Add(new MinLengthValidator(minLength.Value));
            }

            return new ValidatorsContainer<PublicationContext>(validators.ToArray());
        }

        private IValidator<CommentContext> BuildCommentValidator(IServiceProvider provider)
        {
            var validators = new List<IValidator<CommentContext>>
            {
                provider.GetService<ForbiddenWordsValidator>(),
                new MaxLengthValidator(configuration.GetValue<int?>("COMMENT_CONTENT_MAX_LENGTH") ?? 5000)
            };

            var minLength = configuration.GetValue<int?>("COMMENT_CONTENT_MIN_LENGTH");
            if (minLength.HasValue)
            {
                validators.Add(new MinLengthValidator(minLength.Value));
            }

            return new ValidatorsContainer<CommentContext>(validators.ToArray());
        }
    }
}