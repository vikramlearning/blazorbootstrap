export function initialize(elementId, elementName, handle, group, allowSorting, pull, put, filter, dotNetHelper) {
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
            onAdd: (event) => {
                event.item.remove();
                dotNetHelper.invokeMethodAsync('OnAddJS', event.oldDraggableIndex, event.newDraggableIndex);
            },
            onRemove: (event) => {
                if (event.pullMode === 'clone') {
                    event.clone.remove();
                }

                event.item.remove();
                event.from.insertBefore(event.item, event.from.childNodes[event.oldIndex]);

                let fromElName = '';
                let toElName = '';

                let fromEl = document.getElementById(event.from.id);
                if (fromEl)
                    fromElName = fromEl.getAttribute('name');

                let toEl = document.getElementById(event.to.id);
                if (toEl)
                    toElName = toEl.getAttribute('name');

                dotNetHelper.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex, fromElName, toElName);
            },
            onUpdate: (event) => {
                event.item.remove();
                event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);
                dotNetHelper.invokeMethodAsync('OnUpdateJS', event.oldDraggableIndex, event.newDraggableIndex);
            },
            sort: allowSorting
        });
    }
}
