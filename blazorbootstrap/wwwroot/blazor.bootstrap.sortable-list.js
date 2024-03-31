export function initialize(dotNetHelper, elementId, handle, group, allowSorting, pull, put, filter) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;

    if (Sortable) {
        const sortable = Sortable.create(listGroupEl, {
            animation: 150,
            filter: '.bb-sortable-list-item-disabled',
            group: {
                name: group,
                pull: pull,
                put: put
            },
            handle: handle, // handle's class
            onUpdate: (event) => {
                event.item.remove();
                event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);

                dotNetHelper.invokeMethodAsync('OnUpdateJS', event.oldDraggableIndex, event.newDraggableIndex);
            },
            onRemove: (event) => {
                if (event.pullMode === 'clone') {
                    event.clone.remove();
                }

                event.item.remove();
                event.from.insertBefore(event.item, event.from.childNodes[event.oldIndex]);

                dotNetHelper.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex);
            },
            sort: allowSorting
        });
    }
}
