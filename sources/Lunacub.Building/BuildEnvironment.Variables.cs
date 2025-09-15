namespace Caxivitual.Lunacub.Building;

partial class BuildEnvironment {
    /// <summary>
    /// Gets the dictionary that contains dynamic environment variables.
    /// </summary>
    public Dictionary<object, object> EnvironmentVariables { get; }

    /// <summary>
    /// Gets the environment variable associates with the key object.
    /// </summary>
    /// <param name="key">The key value of the variable to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the variable value associate with <paramref name="key"/>, or <see langword="null"/>
    ///     if <paramref name="key"/> hasn't been registered.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if environment contains variable associate with <paramref name="key"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetVariable(object key, [NotNullWhen(true)] out object? value) {
        return EnvironmentVariables.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the environment variable associates with the key object.
    /// </summary>
    /// <param name="key">The key value of the variable to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the variable value associate with <paramref name="key"/>, or <see langword="null"/>
    ///     if <paramref name="key"/> hasn't been registered or the output isn't typed <typeparamref name="T"/>.
    /// </param>
    /// <typeparam name="T">The type of variable to cast to.</typeparam>
    /// <returns>
    ///     <see langword="true"/> if environment contains variable associate with <paramref name="key"/> and is type <typeparamref name="T"/>;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetVariable<T>(object key, [NotNullWhen(true)] out T? value) {
        if (EnvironmentVariables.TryGetValue(key, out object? output) && output is T casted) {
            value = casted;
            return true;
        }
        
        value = default;
        return false;
    }
    
    /// <summary>
    /// Removes the environment value associates with the key object.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>
    ///     <see langword="true"/> if the key is found and the value is removed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveVariable(object key) => EnvironmentVariables.Remove(key);
}