    let serialCount = 0;
    function addSerial() {
        const wrapper = document.getElementById('serialNumbersWrapper');

        
        const div = document.createElement('div');
        div.className = 'input-group mb-2 serial-input';

        
        const input = document.createElement('input');
        input.type = 'text';
        input.name = 'serialNumber[]';
        input.className = 'form-control';
        input.placeholder = 'Serial Number';

        
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-danger btn-sm';
        btn.textContent = '-';
        btn.onclick = function () {
            div.remove();
        };

       
        div.appendChild(input);
        div.appendChild(btn);

        wrapper.appendChild(div);
    }

    function removeSerial(button) {
        button.closest('.serial-input').remove();
    }