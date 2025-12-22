# Kanban Board Overview

This is a five-state kanban system for managing tasks:

- **backlog/** - Tasks planned but not yet ready to work on
- **to-do/** - Planned tasks ready to be started  
- **in-progress/** - Tasks currently being worked on
- **done/** - Completed tasks
- **archived/** - Cancelled, obsolete, or indefinitely deferred tasks

## Task Naming

- Simple tasks: `NNN-task-description.md`
- Complex tasks: `NNN-task-description/task.md` (folder structure)
- All tasks use three-digit numbering (001-999)

## Usage

```bash
kanban                           # Show board (To-Do and In-Progress only)
kanban --show-done               # Show board including Done column
kanban --show-backlog            # Show board including Backlog column
kanban --show-archived           # Show board including Archived column
kanban create "Task title"       # Create task in to-do
kanban move <id> <column>        # Move task to different column
kanban done <id>                 # Move task to done
kanban archive <id>              # Archive task
kanban show <id>                 # Show task details
```

## When to use archived/ vs done/

- **done/** - Task was completed successfully
- **archived/** - Task was NOT completed but is no longer active:
  - Cancelled (decided not to do it)
  - Obsolete (no longer relevant due to other changes)  
  - Superseded (replaced by a different approach)
  - Indefinitely deferred (might revive later, but not actively planned)