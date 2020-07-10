public enum eProgressState
{
    IDLE,
    CREATE,
    ARTIFACTS,
    COPY_GEO,
    COPY_ARTIFACTS,
    WALLSEGMENTS,
    END
}

public enum eArtifactExceptions
{
    NA,
    AE1,
    AE2,
    AE3,
    AE4,
    AE5,
    END
}

public enum eDirections
{
    NORTH,
    SOUTH,
    EAST,
    WEST,
    NA
}

public enum eStateDebuging
{
    PROGRESS,
    ARTIFACTS,
    WALLSEGMENTS,
}

public enum eConsoleLogging
{
    NA,
    DEBUG,
    WARNING,
    ERROR,
    END
}