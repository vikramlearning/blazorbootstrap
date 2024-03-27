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
            sort: allowSorting
        });
    }
}
