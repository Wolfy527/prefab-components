# Extending Live Mirroring

`LiveMirroringSystem` is the stable serialized compatibility component. Preserve its
script GUID, class name, and existing serialized fields when updating the asset.
Add new fields instead of changing field types, and use `FormerlySerializedAs` or
`LiveMirroringMigrationService` when existing serialized data must move.

## Inspector sections

Create a public, non-abstract class with a public parameterless constructor that
implements `ILiveMirroringInspectorSection`.

- Give it a permanent, unique `SectionId`.
- Use `Order` to control its location.
- Use `IsVisible()` for conditional sections.
- Keep `GetSummary()` limited to useful state when collapsed.
- Draw with the shared `PropToolsEditor` controls so redesigning the shared shell
  does not require rewriting the section.

Sections are discovered automatically. Adding one does not require editing
`LiveMirroringSystemEditor` or a central list.

## Mirroring processing

Implement `ILiveMirroringProcessor` for behavior that runs before or after the
stable core scale/pair mirroring pass.

- Give it a permanent, unique `ProcessorId`.
- Choose `BeforeCore` only when the extension must prepare data consumed by the
  core pass. Prefer `AfterCore` for additive behavior.
- Keep processors stateless. Serialized configuration belongs on
  `LiveMirroringSystem` or a future compatible settings container.
- A processor that throws is disabled for the current editor session so it cannot
  repeatedly interrupt the core mirroring system.

## Validation and scene previews

Implement `ILiveMirroringValidationContributor` to add messages above the normal
sections without modifying the main Inspector.

Implement `ILiveMirroringPreviewContributor` when a feature must configure a newly
created ghost or update it after the core preview transform has been applied.
Preview contributors receive the system, target, and hidden preview instance.

## Upload boundary

Live Mirroring is an authoring-only system and removes its generated editor-only
object during play mode and avatar upload. Features required on the uploaded avatar
must bake their runtime result into supported avatar components, animations, or
another upload-time integration before Live Mirroring is stripped.
