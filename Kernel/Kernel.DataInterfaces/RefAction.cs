namespace Kernel.DataInterfaces;

public delegate void RefAction<T, TRef>(in T item1, ref TRef item2);

public delegate void RefAction<T1, T2, TRef2>(in T1 item1, in T2 item2, ref TRef2 item3);

public delegate TRef RefFunc<T, out TRef>(ref T item1);

public delegate TRef RefFunc<T1, T2, out TRef>(in T1 item1, ref T2 item2);