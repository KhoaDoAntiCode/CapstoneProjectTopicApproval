(function () {
    const state = {
        auth: loadAuth(),
        selectedProjectId: null,
        editProjectId: null,
        lastSearch: "",
        projects: []
    };

    const els = {
        loginForm: document.getElementById("loginForm"),
        emailInput: document.getElementById("emailInput"),
        passwordInput: document.getElementById("passwordInput"),
        sessionTitle: document.getElementById("sessionTitle"),
        authBadge: document.getElementById("authBadge"),
        sessionInfo: document.getElementById("sessionInfo"),
        sessionUser: document.getElementById("sessionUser"),
        sessionMeta: document.getElementById("sessionMeta"),
        logoutButton: document.getElementById("logoutButton"),
        navLinks: document.querySelectorAll(".nav-link"),
        actionLinks: document.querySelectorAll(".action-link"),
        screens: {
            overview: document.getElementById("overviewScreen"),
            create: document.getElementById("createScreen"),
            topics: document.getElementById("topicsScreen")
        },
        fillSampleButton: document.getElementById("fillSampleButton"),
        parseForm: document.getElementById("parseForm"),
        docxFileInput: document.getElementById("docxFileInput"),
        projectForm: document.getElementById("projectForm"),
        supervisorsContainer: document.getElementById("supervisorsContainer"),
        studentsContainer: document.getElementById("studentsContainer"),
        addSupervisorButton: document.getElementById("addSupervisorButton"),
        addStudentButton: document.getElementById("addStudentButton"),
        clearFormButton: document.getElementById("clearFormButton"),
        loadProjectsButton: document.getElementById("loadProjectsButton"),
        searchInput: document.getElementById("searchInput"),
        searchButton: document.getElementById("searchButton"),
        projectList: document.getElementById("projectList"),
        projectDetail: document.getElementById("projectDetail"),
        reloadDetailButton: document.getElementById("reloadDetailButton"),
        editProjectButton: document.getElementById("editProjectButton"),
        deleteProjectButton: document.getElementById("deleteProjectButton"),
        regenerateDocxButton: document.getElementById("regenerateDocxButton"),
        toast: document.getElementById("toast"),
        supervisorTemplate: document.getElementById("supervisorTemplate"),
        studentTemplate: document.getElementById("studentTemplate")
    };

    init();

    function init() {
        bindNav();
        bindAuth();
        bindForm();
        bindList();
        addSupervisorRow();
        addStudentRow({ roleInGroup: "Leader" });
        renderAuth();
        loadProjects();
    }

    function bindNav() {
        els.navLinks.forEach((button) => {
            if (button.dataset.screen) {
                button.addEventListener("click", () => setScreen(button.dataset.screen));
            }
        });

        els.actionLinks.forEach((button) => {
            button.addEventListener("click", () => {
                const action = button.dataset.action;
                if (action === "edit-selected") {
                    startEditSelectedProject();
                } else if (action === "regenerate-selected") {
                    regenerateSelectedProjectDocx();
                }
            });
        });
    }

    function bindAuth() {
        els.loginForm.addEventListener("submit", async (event) => {
            event.preventDefault();

            try {
                const response = await apiFetch("/api/auth/login", {
                    method: "POST",
                    body: JSON.stringify({
                        email: els.emailInput.value.trim(),
                        password: els.passwordInput.value
                    })
                }, false);

                state.auth = response.data;
                saveAuth(state.auth);
                renderAuth();
                showToast("Signed in successfully.");
                await loadProjects();
            } catch (error) {
                showToast(error.message || "Login failed.");
            }
        });

        els.logoutButton.addEventListener("click", () => {
            state.auth = null;
            saveAuth(null);
            renderAuth();
            showToast("Signed out.");
        });
    }

    function bindForm() {
        els.addSupervisorButton.addEventListener("click", () => addSupervisorRow());
        els.addStudentButton.addEventListener("click", () => addStudentRow());
        els.fillSampleButton.addEventListener("click", fillSampleData);
        els.clearFormButton.addEventListener("click", resetProjectForm);

        els.parseForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            if (!requireAuth()) {
                return;
            }

            const file = els.docxFileInput.files[0];
            if (!file) {
                showToast("Choose a DOCX file first.");
                return;
            }

            const formData = new FormData();
            formData.append("file", file);

            try {
                const response = await apiFetch("/api/projects/parse", {
                    method: "POST",
                    body: formData,
                    isMultipart: true
                });

                hydrateForm(response.data || {});
                showToast("DOCX parsed and form updated.");
                setScreen("create");
            } catch (error) {
                showToast(error.message || "Could not parse DOCX.");
            }
        });

        els.projectForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            if (!requireAuth()) {
                return;
            }

            try {
                const payload = buildProjectPayload();
                if (state.editProjectId) {
                    const response = await apiFetch(`/api/projects/${state.editProjectId}`, {
                        method: "PUT",
                        body: JSON.stringify(payload)
                    });

                    showToast(response.message || "Project updated successfully.");
                    state.selectedProjectId = state.editProjectId;
                    state.editProjectId = null;
                    await loadProjects();
                    setScreen("topics");
                    await loadProjectDetail(state.selectedProjectId);
                } else {
                    const fileResult = await downloadFetch("/api/projects/create-with-docx", {
                        method: "POST",
                        body: JSON.stringify(payload)
                    });

                    showToast(`Project submitted and ${fileResult.fileName} downloaded.`);
                    setScreen("topics");
                    await loadProjects();
                }

                resetProjectForm();
            } catch (error) {
                showToast(error.message || "Submission failed.");
            }
        });
    }

    function bindList() {
        els.loadProjectsButton.addEventListener("click", loadProjects);
        els.searchButton.addEventListener("click", () => {
            state.lastSearch = els.searchInput.value.trim();
            loadProjects();
        });
        els.reloadDetailButton.addEventListener("click", () => {
            if (state.selectedProjectId) {
                loadProjectDetail(state.selectedProjectId);
            }
        });
        els.editProjectButton.addEventListener("click", startEditSelectedProject);
        els.deleteProjectButton.addEventListener("click", deleteSelectedProject);
        els.regenerateDocxButton.addEventListener("click", regenerateSelectedProjectDocx);
    }

    function renderAuth() {
        const user = state.auth;
        const isOnline = Boolean(user?.token);

        els.authBadge.textContent = isOnline ? "online" : "offline";
        els.authBadge.className = `status-pill ${isOnline ? "live" : "muted"}`;
        els.loginForm.classList.toggle("hidden", isOnline);
        els.sessionInfo.classList.toggle("hidden", !isOnline);
        els.sessionTitle.textContent = isOnline ? "Current Admin" : "Admin Login";

        if (isOnline) {
            els.sessionUser.textContent = `${user.fullName} (${user.role})`;
            els.sessionMeta.textContent = user.email;
        } else {
            els.sessionUser.textContent = "";
            els.sessionMeta.textContent = "";
        }
    }

    function setScreen(screenName) {
        Object.entries(els.screens).forEach(([name, screen]) => {
            screen.classList.toggle("active", name === screenName);
        });

        els.navLinks.forEach((button) => {
            button.classList.toggle("active", button.dataset.screen === screenName);
        });
    }

    function addSupervisorRow(data = {}) {
        const fragment = els.supervisorTemplate.content.cloneNode(true);
        const card = fragment.querySelector(".dynamic-card");
        setDynamicValues(card, data);
        wireRemoveButton(card);
        els.supervisorsContainer.appendChild(card);
    }

    function addStudentRow(data = {}) {
        const fragment = els.studentTemplate.content.cloneNode(true);
        const card = fragment.querySelector(".dynamic-card");
        setDynamicValues(card, data);
        wireRemoveButton(card);
        els.studentsContainer.appendChild(card);
    }

    function wireRemoveButton(card) {
        const removeButton = card.querySelector(".remove-row");
        removeButton.addEventListener("click", () => {
            card.remove();
        });
    }

    function setDynamicValues(card, data) {
        card.querySelectorAll("[data-field]").forEach((input) => {
            const key = input.dataset.field;
            if (!(key in data)) {
                return;
            }

            if (input.type === "checkbox") {
                input.checked = Boolean(data[key]);
            } else {
                input.value = data[key] ?? "";
            }
        });
    }

    function hydrateForm(data) {
        resetProjectForm();

        setNamedValue("semesterId", data.detectedSemesterId || data.semesterId);
        setNamedValue("englishName", data.englishName);
        setNamedValue("vietnameseName", data.vietnameseName);
        setNamedValue("abbreviation", data.abbreviation);
        setNamedValue("className", data.className);
        setNamedValue("durationFrom", formatDateForInput(data.durationFrom));
        setNamedValue("durationTo", formatDateForInput(data.durationTo));
        setNamedValue("profession", data.profession);
        setNamedValue("specialty", data.specialty);
        setNamedValue("registerKind", data.registerKind);
        setNamedValue("context", data.context);
        setNamedValue("proposedSolutions", data.proposedSolutions);
        setNamedValue("functionalRequirements", data.functionalRequirements);
        setNamedValue("nonFunctionalRequirements", data.nonFunctionalRequirements);
        setNamedValue("theoryAndPractice", data.theoryAndPractice);
        setNamedValue("products", data.products);
        setNamedValue("proposedTasks", data.proposedTasks);

        els.supervisorsContainer.innerHTML = "";
        els.studentsContainer.innerHTML = "";

        (data.supervisors || []).forEach((supervisor) => addSupervisorRow(supervisor));
        (data.students || []).forEach((student) => addStudentRow(student));

        if (!els.supervisorsContainer.children.length) {
            addSupervisorRow();
        }

        if (!els.studentsContainer.children.length) {
            addStudentRow({ roleInGroup: "Leader" });
        }
    }

    function buildProjectPayload() {
        return {
            semesterId: getNamedValue("semesterId"),
            englishName: getNamedValue("englishName"),
            vietnameseName: getNamedValue("vietnameseName"),
            abbreviation: getNamedValue("abbreviation"),
            isResearchProject: false,
            isEnterpriseProject: false,
            context: getNamedValue("context"),
            proposedSolutions: getNamedValue("proposedSolutions"),
            functionalRequirements: getNamedValue("functionalRequirements"),
            nonFunctionalRequirements: getNamedValue("nonFunctionalRequirements"),
            theoryAndPractice: getNamedValue("theoryAndPractice"),
            products: getNamedValue("products"),
            proposedTasks: getNamedValue("proposedTasks"),
            className: getNamedValue("className"),
            durationFrom: normalizeDate(getNamedValue("durationFrom")),
            durationTo: normalizeDate(getNamedValue("durationTo")),
            profession: getNamedValue("profession"),
            specialty: getNamedValue("specialty"),
            registerKind: getNamedValue("registerKind"),
            supervisors: collectDynamicRows(els.supervisorsContainer),
            students: collectDynamicRows(els.studentsContainer)
        };
    }

    function collectDynamicRows(container) {
        return Array.from(container.querySelectorAll(".dynamic-card"))
            .map((card, index) => {
                const row = { displayOrder: index + 1 };
                card.querySelectorAll("[data-field]").forEach((input) => {
                    const key = input.dataset.field;
                    row[key] = input.type === "checkbox" ? input.checked : input.value.trim();
                });
                return row;
            })
            .filter((row) => row.fullName);
    }

    async function loadProjects() {
        if (!state.auth?.token) {
            renderProjectList([]);
            return;
        }

        try {
            const query = new URLSearchParams({
                page: "1",
                pageSize: "20"
            });

            if (state.lastSearch) {
                query.set("search", state.lastSearch);
            }

            const response = await apiFetch(`/api/projects?${query.toString()}`, { method: "GET" });
            const paged = response.data || {};
            state.projects = paged.items || [];
            renderProjectList(state.projects);
        } catch (error) {
            renderProjectList([]);
            showToast(error.message || "Could not load project list.");
        }
    }

    function renderProjectList(items) {
        if (!items.length) {
            els.projectList.innerHTML = `<div class="detail-empty">No topics yet. Sign in and create one from the form.</div>`;
            return;
        }

        els.projectList.innerHTML = items.map((item) => {
            const statusClass = String(item.status || "").toLowerCase();
            return `
                <article class="project-row">
                    <div>
                        <h4>${escapeHtml(item.englishName || "Untitled topic")}</h4>
                        <p>${escapeHtml(item.vietnameseName || "")}</p>
                    </div>
                    <div>
                        <p><strong>${escapeHtml(item.projectCode || "-")}</strong></p>
                        <p>${escapeHtml(item.semesterId || "-")}</p>
                    </div>
                    <div>
                        <span class="status-pill ${statusClass || "pending"}">${escapeHtml(item.status || "Pending")}</span>
                    </div>
                    <button class="ghost-button small" type="button" data-open-project="${item.id}">Open</button>
                </article>
            `;
        }).join("");

        els.projectList.querySelectorAll("[data-open-project]").forEach((button) => {
            button.addEventListener("click", () => {
                const id = button.getAttribute("data-open-project");
                state.selectedProjectId = id;
                loadProjectDetail(id);
            });
        });
    }

    async function loadProjectDetail(projectId) {
        if (!projectId || !requireAuth()) {
            return;
        }

        try {
            const [projectResponse, reviewsResponse] = await Promise.all([
                apiFetch(`/api/projects/${projectId}`, { method: "GET" }),
                apiFetch(`/api/projects/${projectId}/reviews`, { method: "GET" })
            ]);

            renderProjectDetail(projectResponse.data || {}, reviewsResponse.data || []);
        } catch (error) {
            showToast(error.message || "Could not load project detail.");
        }
    }

    function renderProjectDetail(project, reviews) {
        const supervisors = (project.supervisors || []).map((item) => item.fullName).join(", ") || "No supervisor";
        const students = (project.students || []).map((item) => `${item.fullName} (${item.roleInGroup || "Member"})`).join(", ") || "No students";
        const reviewMarkup = reviews.length
            ? reviews.map((review) => `<p>${escapeHtml(review.decision)} - ${escapeHtml(review.comment || "No comment")}</p>`).join("")
            : `<p>No reviews yet.</p>`;

        els.projectDetail.innerHTML = `
            <div class="detail-grid">
                <section class="detail-section">
                    <h4>Identity</h4>
                    <p><strong>${escapeHtml(project.englishName || "-")}</strong></p>
                    <p>${escapeHtml(project.vietnameseName || "-")}</p>
                    <p>Code: ${escapeHtml(project.projectCode || "-")}</p>
                    <p>Semester: ${escapeHtml(project.semesterId || "-")}</p>
                </section>
                <section class="detail-section">
                    <h4>Group Setup</h4>
                    <p>Class: ${escapeHtml(project.className || "-")}</p>
                    <p>Profession: ${escapeHtml(project.profession || "-")}</p>
                    <p>Specialty: ${escapeHtml(project.specialty || "-")}</p>
                    <p>Register Kind: ${escapeHtml(project.registerKind || "-")}</p>
                </section>
                <section class="detail-section full">
                    <h4>Supervisor</h4>
                    <p>${escapeHtml(supervisors)}</p>
                </section>
                <section class="detail-section full">
                    <h4>Students</h4>
                    <p>${escapeHtml(students)}</p>
                </section>
                <section class="detail-section full">
                    <h4>Context</h4>
                    <p>${escapeHtml(project.context || "-")}</p>
                </section>
                <section class="detail-section">
                    <h4>Functional Requirements</h4>
                    <p>${escapeHtml(project.functionalRequirements || "-")}</p>
                </section>
                <section class="detail-section">
                    <h4>Non-functional Requirements</h4>
                    <p>${escapeHtml(project.nonFunctionalRequirements || "-")}</p>
                </section>
                <section class="detail-section">
                    <h4>Products</h4>
                    <p>${escapeHtml(project.products || "-")}</p>
                </section>
                <section class="detail-section">
                    <h4>Proposed Tasks</h4>
                    <p>${escapeHtml(project.proposedTasks || "-")}</p>
                </section>
                <section class="detail-section full">
                    <h4>Review History</h4>
                    ${reviewMarkup}
                </section>
            </div>
        `;
    }

    async function startEditSelectedProject() {
        if (!state.selectedProjectId || !requireAuth()) {
            showToast("Select a topic first.");
            return;
        }

        try {
            const response = await apiFetch(`/api/projects/${state.selectedProjectId}`, { method: "GET" });
            const project = response.data || {};
            state.editProjectId = project.id;
            hydrateForm(project);
            setScreen("create");
            showToast(`Editing ${project.projectCode || "selected project"}.`);
        } catch (error) {
            showToast(error.message || "Could not load project for editing.");
        }
    }

    async function deleteSelectedProject() {
        if (!state.selectedProjectId || !requireAuth()) {
            showToast("Select a topic first.");
            return;
        }

        const confirmed = window.confirm("Delete this topic? This cannot be undone.");
        if (!confirmed) {
            return;
        }

        try {
            const response = await apiFetch(`/api/projects/${state.selectedProjectId}`, {
                method: "DELETE"
            });

            state.selectedProjectId = null;
            state.editProjectId = null;
            els.projectDetail.innerHTML = "Select a topic from the list to inspect full submission data.";
            await loadProjects();
            showToast(response.message || "Project deleted successfully.");
        } catch (error) {
            showToast(error.message || "Could not delete project.");
        }
    }

    async function regenerateSelectedProjectDocx() {
        if (!state.selectedProjectId || !requireAuth()) {
            showToast("Select a topic first.");
            return;
        }

        try {
            const fileResult = await downloadFetch(`/api/projects/${state.selectedProjectId}/regenerate-docx`, { method: "POST" });
            showToast(`${fileResult.fileName} downloaded.`);
        } catch (error) {
            showToast(error.message || "Could not generate DOCX.");
        }
    }

    function fillSampleData() {
        hydrateForm({
            detectedSemesterId: "SP26",
            englishName: "AI-Powered Educational Plush Toy Store",
            vietnameseName: "Cua Hang Gau Bong Hoc Tap Tich Hop AI",
            abbreviation: "AIEPTS",
            className: "SE1848",
            durationFrom: "2025-12-01",
            durationTo: "2026-05-01",
            profession: "Software Engineer",
            specialty: "SE",
            registerKind: "Students",
            context: "Build an ecommerce platform where parents can customize educational plush toys and receive AI recommendations based on a child learning profile.",
            proposedSolutions: "Provide product customization, AI recommendation, voice interaction, and a lightweight admin dashboard for inventory and reporting.",
            functionalRequirements: "Product customization, AI recommendation engine, product catalog, shopping cart, checkout, order tracking, and admin dashboard.",
            nonFunctionalRequirements: "Responsive design, scalable AI services, secure communication, and high availability during peak periods.",
            theoryAndPractice: "A working web application plus a dashboard for business control and learning-content personalization.",
            products: "Working web app, dashboard, prototype dataset, sample articles, sample labs, and metadata.",
            proposedTasks: "Research, architecture, UI design, backend APIs, AI recommendation, testing, documentation.",
            supervisors: [
                {
                    fullName: "Lam Huu Khanh Phuong",
                    email: "phuonglhk@fpt.edu.vn",
                    title: "Mr.",
                    isPrimary: true
                }
            ],
            students: [
                { fullName: "Nguyen Thanh Danh", studentCode: "SE181519", phone: "0947041559", email: "danhntse183755@fpt.edu.vn", roleInGroup: "Leader" },
                { fullName: "Pham Minh Quan", studentCode: "SE184225", phone: "0375727245", email: "quanpmse184225@fpt.edu.vn", roleInGroup: "Member" }
            ]
        });
        setScreen("create");
        showToast("Demo form data loaded.");
    }

    function resetProjectForm() {
        els.projectForm.reset();
        state.editProjectId = null;
        els.supervisorsContainer.innerHTML = "";
        els.studentsContainer.innerHTML = "";
        addSupervisorRow();
        addStudentRow({ roleInGroup: "Leader" });
    }

    function requireAuth() {
        if (!state.auth?.token) {
            showToast("Sign in first to use the API.");
            return false;
        }

        return true;
    }

    async function apiFetch(url, options = {}, requireToken = true) {
        const headers = {};
        const isMultipart = options.isMultipart === true;

        if (!isMultipart) {
            headers["Content-Type"] = "application/json";
        }

        if (requireToken && state.auth?.token) {
            headers["Authorization"] = `Bearer ${state.auth.token}`;
        }

        const response = await fetch(url, {
            method: options.method || "GET",
            headers,
            body: options.body
        });

        const payload = await response.json().catch(() => ({}));
        if (!response.ok || payload.success === false) {
            const message = payload.message || payload.title || "Request failed.";
            throw new Error(message);
        }

        return payload;
    }

    async function downloadFetch(url, options = {}, requireToken = true) {
        const headers = {};
        if (requireToken && state.auth?.token) {
            headers["Authorization"] = `Bearer ${state.auth.token}`;
        }

        if (!(options.body instanceof FormData)) {
            headers["Content-Type"] = "application/json";
        }

        const response = await fetch(url, {
            method: options.method || "GET",
            headers,
            body: options.body
        });

        if (!response.ok) {
            let message = "Request failed.";
            try {
                const payload = await response.json();
                message = payload.message || message;
            } catch {
                // Ignore non-JSON error payloads.
            }
            throw new Error(message);
        }

        const blob = await response.blob();
        const fileName = getDownloadFileName(response) || "capstone-project.docx";
        const downloadUrl = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = downloadUrl;
        anchor.download = fileName;
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        URL.revokeObjectURL(downloadUrl);

        return { fileName };
    }

    function showToast(message) {
        els.toast.textContent = message;
        els.toast.classList.remove("hidden");
        clearTimeout(showToast.timer);
        showToast.timer = setTimeout(() => {
            els.toast.classList.add("hidden");
        }, 3600);
    }

    function getNamedValue(name) {
        return els.projectForm.elements.namedItem(name)?.value?.trim() || "";
    }

    function setNamedValue(name, value) {
        const field = els.projectForm.elements.namedItem(name);
        if (field) {
            field.value = value || "";
        }
    }

    function normalizeDate(value) {
        return value || null;
    }

    function formatDateForInput(value) {
        return value ? String(value).slice(0, 10) : "";
    }

    function escapeHtml(value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function saveAuth(data) {
        if (!data) {
            localStorage.removeItem("capstone-auth");
            return;
        }

        localStorage.setItem("capstone-auth", JSON.stringify(data));
    }

    function loadAuth() {
        const raw = localStorage.getItem("capstone-auth");
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch {
            localStorage.removeItem("capstone-auth");
            return null;
        }
    }

    function getDownloadFileName(response) {
        const disposition = response.headers.get("Content-Disposition") || "";
        const utfMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (utfMatch) {
            return decodeURIComponent(utfMatch[1]);
        }

        const asciiMatch = disposition.match(/filename="?([^"]+)"?/i);
        return asciiMatch ? asciiMatch[1] : null;
    }
})();
