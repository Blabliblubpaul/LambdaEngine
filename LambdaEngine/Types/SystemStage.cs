namespace LambdaEngine.Types;

public enum SystemStage {
    FRAME_START,
    EARLY_UPDATE,
    FIXED_UPDATE,
    UPDATE,
    RENDER,
    ENTITY_DESTRUCTION
}