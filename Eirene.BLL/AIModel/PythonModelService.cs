using Microsoft.Extensions.Options;
using Python.Runtime;
using System.IO;

namespace Eirene.BLL.AIModel;

public class PythonModelService : IPythonModelService, IDisposable
{
    private readonly dynamic _predictor;
    private bool _disposed;

    public PythonModelService(IOptions<PythonSettings> settings)
    {
        var pythonSettings = settings.Value;

        // Ensure DLL path is set for pythonnet
        if (!string.IsNullOrEmpty(pythonSettings.DllPath))
        {
            Runtime.PythonDLL = pythonSettings.DllPath;
        }

        // Initialize PythonEngine if not already initialized
        if (!PythonEngine.IsInitialized)
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads(); // Allow other threads to use Python
        }

        using (Py.GIL()) // Acquire the Global Interpreter Lock
        {
            // Add script directory to sys.path
            dynamic sys = Py.Import("sys");
            sys.path.append(pythonSettings.ScriptDirectory);

            // Import the script
            dynamic pythonScript = Py.Import("python_inference");

            // Instantiate the model predictor
            _predictor = pythonScript.ModelPredictor(pythonSettings.ModelPath);
        }
    }

    public int PredictMentalHealthIssue(string text)
    {
        using (Py.GIL())
        {
            int result = _predictor.predict(text);
            return result;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            using (Py.GIL())
            {
                if (_predictor != null)
                {
                    _predictor.Dispose();
                }
            }

            if (PythonEngine.IsInitialized)
            {
                PythonEngine.Shutdown();
            }

            _disposed = true;
        }
    }
}
