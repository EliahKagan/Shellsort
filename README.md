# Shellsort - comparing Shellsort by gap sequence and to other sorts

*Written in 2018 by Eliah Kagan \<degeneracypressure@gmail.com\>. Documentation
added in 2021.*

*To the extent possible under law, the author(s) have dedicated all copyright
and related and neighboring rights to this software to the public domain
worldwide. This software is distributed without any warranty.*

*You should have received a copy of the CC0 Public Domain Dedication along with
this software. If not, see
<http://creativecommons.org/publicdomain/zero/1.0/>.*

This small C# program (written as a LINQPad query) implements Shellsort with a
few different gap sequences, and two implementation techniques (the usual
&ldquo;hard-coded&rdquo; approach, and by insertion-sorting a `GapView`
abstraction of the input).

The gap sequences implemented are currently Hibbard 1963, Pratt 3-smooth, and
Tokuda 1992.

These are benchmarked, together with insertion sort (for small sizes),
heapsort, quicksort (with Lomuto partitioning), and the framework-provided
introsort, for comparison.

See also [**Sorts**](https://github.com/EliahKagan/Sorts), a C++ program that
is similar to this, but with more gap sequences and more other sorting
algorithms, and with commented citations and explanations of the gap sequences.
