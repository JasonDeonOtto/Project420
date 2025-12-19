You are working on an enterprise-grade Proof-of-Concept (PoC) system called Project420.

Your task is to create a **PoC Evidence Runner** that provides
**undeniable, client-facing proof** of core system laws.

This Evidence Runner must:
- Execute hostile-proof scenarios
- Produce immediate human-readable console output
- Optionally generate a single, structured evidence report file
- Clearly show intent → action → enforcement → result

⚠️ This is NOT a test framework
⚠️ This is NOT production logging
⚠️ This is NOT observability or telemetry
This is **forensic evidence** — equivalent to “show your work” in mathematics.

--------------------------------
PRIMARY GOALS
--------------------------------
1. Provide a single executable entry point that:
   - Runs all hostile evidence checks
   - Outputs PASS / FAIL to the console
   - Can be executed live in front of a client

2. Strengthen credibility by optionally producing
   a **single evidence report file** per run that:
   - Explains what was attempted
   - Explains which rule was enforced
   - Explains why the system accepted or rejected the action

3. Reuse existing business services and rules.
   - Do NOT duplicate logic
   - Do NOT mock core behaviour
   - Do NOT reference test frameworks

--------------------------------
PROJECT STRUCTURE
--------------------------------
Create a new console project:

tests/
 └─ Project420.ProofRunner/
     ├─ Program.cs
     ├─ Evidence/
     │   ├─ ImmutabilityEvidence.cs
     │   ├─ CompensationEvidence.cs
     │   ├─ ReplayEvidence.cs
     │   └─ TraceabilityEvidence.cs
     ├─ Reporting/
     │   ├─ EvidenceReport.cs
     │   └─ EvidenceReportWriter.cs
     └─ Infrastructure/
         └─ EvidenceDbContextFactory.cs

--------------------------------
EXECUTION MODEL
--------------------------------
When executed, the program must:

1. Print concise console output such as:

--------------------------------
Project420 Evidence Check
--------------------------------
✔ Stock history is immutable
✔ Corrections create compensating movements
✔ Stock can be reconstructed as-of a date
✔ All movements are traceable to an actor

Evidence Status: PASS
--------------------------------

2. Optionally (always enabled by default):
   - Generate **one structured evidence report file**
   - The file must be human-readable
   - The file must describe the flow, not dump internals

--------------------------------
EVIDENCE REPORT FORMAT
--------------------------------
Generate **exactly one** report file per execution.

Preferred format: **XML** (JSON acceptable if justified)

The report must follow this narrative structure:

<EvidenceRun>
  <Metadata>
    <RunId />
    <TimestampUtc />
    <Environment>PoC</Environment>
    <ExecutedBy>Project420.ProofRunner</ExecutedBy>
  </Metadata>

  <Evidence>
    <Rule name="Immutability">
      <Intent>What rule is being tested</Intent>
      <Action>What action was attempted</Action>
      <ExpectedOutcome>What should happen</ExpectedOutcome>
      <ActualOutcome>What actually happened</ActualOutcome>
      <Result>PASS | FAIL</Result>
    </Rule>
  </Evidence>
</EvidenceRun>

The report must explain behaviour.
It must NOT contain stack traces, SQL dumps, or debug noise.

--------------------------------
EVIDENCE RULES TO IMPLEMENT
--------------------------------

1. Immutability Evidence
   - Attempt to update or delete a stock movement
   - Operation must fail
   - Original record must remain unchanged

2. Compensation Evidence
   - Create an incorrect movement
   - Apply a compensating movement
   - Verify both records exist
   - Verify net stock is correct

3. Replay Evidence
   - Calculate stock as-of two different dates
   - Results must differ
   - Results must be derived from movements only

4. Traceability Evidence
   - Create an action with ActorContext + CorrelationId
   - Verify all resulting movements reference both

--------------------------------
CONSTRAINTS (STRICT)
--------------------------------
- Do NOT introduce authentication
- Do NOT add UI
- Do NOT add logging frameworks
- Do NOT stream logs
- Do NOT optimize for performance
- Do NOT generalize this into production tooling
- Do NOT expose test framework concepts

--------------------------------
STYLE & TONE
--------------------------------
- Calm
- Deterministic
- Law-driven
- Explicit
- Minimal

Console output is the primary proof.
The evidence report is secondary, forensic support only.

--------------------------------
DELIVERABLE
--------------------------------
Provide:
- Program.cs
- Each Evidence class with clear execution methods
- Evidence report model and writer
- Example console output
- Example evidence report file

Do not explain concepts.
Produce only code and minimal usage instructions.

This Evidence Runner must be suitable for
live, hostile, client-facing demonstrations
where explanation is replaced by execution.
