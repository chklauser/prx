namespace Prexonite.Modular;

public class ModuleConflictException : Exception
{
    public ModuleConflictException(string message, Module module1, Module module2)
        : base(_appendModules(message, module1, module2))
    {
        Module1 = module1;
        Module2 = module2;
    }

    public ModuleConflictException(string message, Exception inner, Module module1, Module module2)
        : base(_appendModules(message, module1, module2), inner)
    {
        Module1 = module1;
        Module2 = module2;
    }

    static string _appendModules(string message, Module? m1, Module? m2)
    {
        if (m1 == null && m2 == null)
            return message;
        return string.Format(
            "{1}{0}Conflict over module {2}.",
            Environment.NewLine,
            message,
            (m1 ?? m2)!.Name
        );
    }

    public Module Module1 { get; set; }
    public Module Module2 { get; set; }
}
