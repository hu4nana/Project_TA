public sealed class PlayerContext
{
    public Player Player { get; }
    public PlayerInputReader Input { get; }
    public PlayerMotor Motor { get; }
    public PlayerCombat Combat { get; }
    public PlayerDefense Defense { get; }
    public PlayerInteractor Interactor { get; }
    public PlayerResourceController Resources { get; }
    public PlayerSkillCaster SkillCaster { get; }

    public PlayerContext(
        Player player,
        PlayerInputReader input,
        PlayerMotor motor,
        PlayerCombat combat,
        PlayerDefense defense,
        PlayerInteractor interactor,
        PlayerResourceController resources,
        PlayerSkillCaster skillCaster)
    {
        Player = player;
        Input = input;
        Motor = motor;
        Combat = combat;
        Defense = defense;
        Interactor = interactor;
        Resources = resources;
        SkillCaster = skillCaster;
    }
}
