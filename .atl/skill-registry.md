# Skill Registry — biblioteca-virtual

Generated: 2026-05-17  
Scope: project + user skills  
Project skills found: none  
Project convention files found: none

## Resolution Rules

- Prefer project-level skills over user-level skills when names collide.
- Ignore SDD executor skills (`sdd-*`), `_shared`, and `skill-registry` for project standards resolution.
- Load a listed skill when its trigger matches the task or file context.
- If no listed skill matches, proceed with repository conventions and the active SDD phase rules.

## Skills

### branch-pr

- Trigger: creating, opening, or preparing pull requests for review.
- Path: `/home/varocode/.config/opencode/skills/branch-pr/SKILL.md`
- Every PR must link an approved issue and include exactly one `type:*` label.
- Branch names must match `type/description` using conventional commit types and lowercase safe characters.
- PR bodies must include linked issue, PR type, summary, changes table, test plan, and checklist.
- Commit messages must follow Conventional Commits.
- Never add `Co-Authored-By` trailers.

### chained-pr

- Trigger: PRs over 400 changed lines, stacked PRs, review slices, reviewer-load control.
- Path: `/home/varocode/.config/opencode/skills/chained-pr/SKILL.md`
- Split PRs over 400 changed lines unless a maintainer accepts `size:exception`.
- Keep each PR to one deliverable work unit and about ≤60 minutes of review.
- Preserve tests and docs with the work unit they verify.
- Use stacked PRs when slices can land independently; use feature-branch chains when integration must happen before main.
- Every child PR needs chain context and a dependency diagram marking the current PR.

### cognitive-doc-design

- Trigger: writing guides, READMEs, RFCs, onboarding, architecture, or review-facing docs.
- Path: `/home/varocode/.config/opencode/skills/cognitive-doc-design/SKILL.md`
- Lead with the answer, then provide progressive detail.
- Use chunking, signposting, tables, checklists, and examples to reduce recall burden.
- For review docs, state what to review first and what is out of scope.
- Keep sections focused on one decision or work unit.

### comment-writer

- Trigger: PR feedback, issue replies, reviews, Slack messages, or GitHub comments.
- Path: `/home/varocode/.config/opencode/skills/comment-writer/SKILL.md`
- Start with the actionable point and keep comments short.
- Be warm, direct, and explain the technical reason when asking for changes.
- Match the thread/user language.
- Avoid pile-ons and avoid em dashes.

### go-testing

- Trigger: Go tests, go test coverage, Bubbletea teatest, golden files.
- Path: `/home/varocode/.config/opencode/skills/go-testing/SKILL.md`
- Prefer table-driven tests and assert behavior, state, errors, and side effects.
- Use `t.TempDir()` for filesystem tests and skippable integration tests for external commands.
- Test Bubbletea `Model.Update()` directly unless full interaction requires `teatest`.
- Keep golden files deterministic and rerun tests without update mode.

### issue-creation

- Trigger: creating GitHub issues, bug reports, or feature requests.
- Path: `/home/varocode/.config/opencode/skills/issue-creation/SKILL.md`
- Use issue templates; blank issues are disabled.
- Search for duplicates before creating an issue.
- New issues receive `status:needs-review`; a maintainer must add `status:approved` before PR work.
- Questions belong in Discussions, not issues.

### judgment-day

- Trigger: `judgment day`, dual review, adversarial review, `juzgar`.
- Path: `/home/varocode/.config/opencode/skills/judgment-day/SKILL.md`
- Resolve project skills before review and inject the same standards into both judge prompts.
- Use two blind judges in parallel and synthesize only after both complete.
- Confirm issues only when both judges agree on CRITICAL or real WARNING findings.
- Ask before fixing Round 1 confirmed issues; re-judge after fixes.
- Terminal states are only approved or escalated.

### skill-creator

- Trigger: new skills, agent instructions, documenting AI usage patterns.
- Path: `/home/varocode/.config/opencode/skills/skill-creator/SKILL.md`
- Create skills only for reusable AI guidance, not one-off documentation.
- Skills must be LLM-first instruction contracts with valid frontmatter.
- Use required sections: Activation Contract, Hard Rules, Decision Gates, Execution Steps, Output Contract, References.
- Keep skill bodies concise and move examples/schemas/detail into local references or assets.
- Register project skills in project guidance when created.

### work-unit-commits

- Trigger: implementation, commit splitting, chained PRs, keeping tests and docs with code.
- Path: `/home/varocode/.config/opencode/skills/work-unit-commits/SKILL.md`
- Commit by deliverable behavior, fix, migration, or docs unit, not by file type.
- Keep tests with the behavior and docs with the user-visible change.
- Each commit should be reviewable, rollbackable, and tell a coherent story.
- If SDD tasks forecast >400 changed lines, group work into chained PR slices before implementation.

## Project Notes

- The repository currently contains only `.atl/` and `PRD_Biblioteca_Virtual.md`; no application code, package manifests, CI, or test configs were present during registry generation.
- The PRD plans a full-stack app using ASP.NET Core Web API, EF Core, PostgreSQL, React, Vite, and Tailwind CSS, but those are not yet implemented files.
