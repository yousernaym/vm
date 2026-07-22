using Xunit;

// This suite shares process-global state across test classes and can't run classes in parallel:
//   - Project's static draw-host override (Project.SetDrawHost), exercised by ProjectTempoTests;
//   - the process-global Media Foundation / FluidSynth native state used by the Integration tests.
// xUnit only serializes tests *within* a collection, so a per-collection attribute can't stop a
// class that mutates the global from racing other classes. Disable cross-collection parallelism
// for the whole assembly instead; the suite is small and the heavy tests are native/I/O-bound, so
// the wall-clock cost is negligible. (The Integration collections keep their own
// DisableParallelization so they stay serial even if this is ever relaxed.)
[assembly: CollectionBehavior(DisableTestParallelization = true)]
