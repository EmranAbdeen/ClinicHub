const doctorSelect = document.getElementById("doctorSelect");
const dateSelect = document.getElementById("dateSelect");
const timeSelect = document.getElementById("timeSelect");
const timeMessage = document.getElementById("timeMessage");

const submitButton =
    document.getElementById("bookButton") || document.getElementById("createButton");


doctorSelect.addEventListener("change", async function () {

    const doctorId = this.value;

    dateSelect.innerHTML = '<option value="">Select Date</option>';
    timeSelect.innerHTML = '<option value="">Select Date First</option>';

    dateSelect.disabled = true;
    timeSelect.disabled = true;
    submitButton.disabled = true;

    if (!doctorId) {
        dateSelect.innerHTML =
            '<option value="">Select Doctor First</option>';

        timeMessage.textContent =
            "Select a doctor to see available dates.";

        return;
    }

    try {

        const response = await fetch(
            `/Appointments/GetAvailableDates?doctorId=${doctorId}`
        );

        const dates = await response.json();

        if (dates.length === 0) {
            dateSelect.innerHTML =
                '<option value="">No available dates</option>';

            timeMessage.textContent =
                "This doctor has no available schedules.";

            return;
        }

        dates.forEach(date => {

            dateSelect.innerHTML +=
                `<option value="${date.value}">
                    ${date.text}
                </option>`;

        });

        dateSelect.disabled = false;

        timeMessage.textContent =
            "Select a date to see available times.";

    }
    catch {
        timeMessage.textContent =
            "Unable to load available dates.";
    }
});


dateSelect.addEventListener("change", async function () {

    const doctorId = doctorSelect.value;
    const date = this.value;

    timeSelect.innerHTML =
        '<option value="">Select Time</option>';

    timeSelect.disabled = true;
    submitButton.disabled = true;

    if (!doctorId || !date) {
        timeMessage.textContent =
            "Select a date to see available times.";

        return;
    }

    try {

        const response = await fetch(
            `/Appointments/GetAvailableSlots?doctorId=${doctorId}&date=${date}`
        );

        const slots = await response.json();

        if (slots.length === 0) {
            timeSelect.innerHTML =
                '<option value="">No available times</option>';

            timeMessage.textContent =
                "There are no available appointment times for this date.";

            return;
        }

        slots.forEach(time => {

            timeSelect.innerHTML +=
                `<option value="${time}">
                    ${time}
                </option>`;

        });

        timeSelect.disabled = false;

        timeMessage.textContent =
            "Choose an available appointment time.";

    }
    catch {
        timeMessage.textContent =
            "Unable to load available times.";
    }
});


timeSelect.addEventListener("change", function () {

    submitButton.disabled = !this.value;

});

