export const ProjectTaskStatus = Object.freeze({
  ToDo: 0,
  InProgress: 1,
  Done: 2,
});

export const projectTaskStatusLabel = (status) => {
  switch (status) {
    case ProjectTaskStatus.ToDo:
      return 'To Do';
    case ProjectTaskStatus.InProgress:
      return 'In Progress';
    case ProjectTaskStatus.Done:
      return 'Done';
    default:
      return 'Unknown';
  }
};
