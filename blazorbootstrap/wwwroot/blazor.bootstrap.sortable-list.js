export function initialize(dotNetHelper, elementId, handle, group, pull, put) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;

    if (Sortable) {
        const sortable = Sortable.create(listGroupEl, {
            animation: 150,
            group: {
                name: group,
                pull: pull,
                put: put
            },
            handle: handle, // handle's class
        });
    }
}
