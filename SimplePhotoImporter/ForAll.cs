namespace SimplePhotoImporter;

public static class ForAllExtension
{
  public static void AsParallelOrSingleAndForAll<T>(this IEnumerable<T> source, Action<T> action, int threadCount)
  {
    if (threadCount == 1)
      foreach (var item in source)
        action(item);
    else
      source.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(action);
  }
}
