namespace DUMPSYM.Extensions;

public delegate T? Selector<in TNode, out T>(TNode node);