#!/bin/bash

# Remove old benchmark tests for memory
echo -n "Attempting to remove old benchmark files..."
rm massif.out.* >/dev/null 2>&1
echo "done"

# Create the benchmark executable with debugging symbols, but do not run it. We
# don't want valgrind to profile the compiler, so we have the "--no-run" flag. We
# also need debugging symbols so valgrind can track down source code
# appropriately. It blows my mind to this day that compiling with optimizations +
# debugging symbols is a thing. For so long I thought they were mutually
# exclusive.
RUSTFLAGS="-g" cargo bench  --no-run

# Now find the created benchmark executable. I tend to prefix my benchmark
# names with 'bench' to easily identify them
# BENCH=`ls -t target/release/bench* | head -1`

BENCH=`find target/release/bench* -maxdepth 1 -perm -111 -type f | head -1`

# Let's say this was the executable
# BENCH="./target/release/bench_bits-430e4e0f5d361f1f"

# Now identify a single test that you want profiled. Test identifiers are
# printed in the console output, so I'll use the one that I posted earlier
T_IDS[0]="p7_naive"
T_IDS[1]="p7_memoize"
T_IDS[2]="p7_tail_recursive"
T_IDS[3]="p7_tail_recursive_ish"

# Have valgrind profile criterion running our benchmark for 10 seconds
#valgrind --tool=callgrind \
#         --dump-instr=yes \
#         --collect-jumps=yes \
#         --simulate-cache=yes \
#         $BENCH --bench --profile-time 10 $T_ID

for item in ${T_IDS[*]}
do
	printf "running test %s\n" $item
	valgrind --tool=massif --stacks=yes \
		--massif-out-file=./massif.out.$item \
		$BENCH --bench --profile-time 60 "$item/12131415161718191010918171615145141313121"
done

# valgrind outputs a callgrind.out.<pid>. We can analyze this with kcachegrind
#massif-visualizer
