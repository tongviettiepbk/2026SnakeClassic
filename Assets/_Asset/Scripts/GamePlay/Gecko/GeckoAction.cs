
public enum ActionNode
{
    RESET_VALUE = 0,
    SET_VALUE = 1,

}

public class GeckoAction 
{
    public StateGecko stateNext;
    public NodeMap nodeTarget;
    public NodeMap nodeFail;
}
