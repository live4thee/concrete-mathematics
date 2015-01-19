# The Josephus Problem

def odd(n):
    return (n % 2) == 1

def even(n):
    return (n % 2) == 0

def josephus(n):
    assert n > 0
    if (n == 1): return 1
    if (even(n)): return 2*josephus(n/2) - 1
    return 2*josephus(n/2) + 1

def round_up_power2(n):
    assert n >= 0
    return 1 << n.bit_length()

# Logic left rotate by 1 bit
def better_josephus(n):
    assert n > 0
    return 1 + ( (n << 1) % round_up_power2(n) )
