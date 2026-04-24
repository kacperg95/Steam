using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QuizEngine.QuestionGenerator;

namespace QuizEngine;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizEngine(this IServiceCollection services)
    {
        services.AddScoped<QuestionFactory>();
        services.AddSingleton<RoomManager>();

        var assembly = typeof(DependencyInjection).Assembly;
        var generatorInterface = typeof(IQuestionGenerator);
        var generatorTypes = assembly.GetTypes()
            .Where(t => generatorInterface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in generatorTypes)
            services.AddScoped(generatorInterface, type);

        return services;
    }
}
